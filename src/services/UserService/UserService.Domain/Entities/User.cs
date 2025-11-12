using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.Primitives;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// This is an aggregate root that manages user identity and authentication.
/// </summary>
public sealed class User : AggregateRoot<Guid>
{
  private readonly List<UserSession> _sessions = new();

  /// <summary>
  /// Gets the user's email address.
  /// </summary>
  public Email Email { get; private set; }

  /// <summary>
  /// Gets the user's hashed password.
  /// </summary>
  public Password Password { get; private set; }

  /// <summary>
  /// Gets the user's full name.
  /// </summary>
  public string FullName { get; private set; }

  /// <summary>
  /// Gets a value indicating whether the user's email has been verified.
  /// </summary>
  public bool IsEmailVerified { get; private set; }

  /// <summary>
  /// Gets a value indicating whether the user account is active.
  /// </summary>
  public bool IsActive { get; private set; }

  /// <summary>
  /// Gets the number of failed login attempts.
  /// </summary>
  public int FailedLoginAttempts { get; private set; }

  /// <summary>
  /// Gets the date and time when the account was locked out (if locked).
  /// </summary>
  public DateTime? LockedOutUntil { get; private set; }

  /// <summary>
  /// Gets the date and time of the last login.
  /// </summary>
  public DateTime? LastLoginAt { get; private set; }

  /// <summary>
  /// Gets the collection of user sessions.
  /// </summary>
  public IReadOnlyCollection<UserSession> Sessions => _sessions.AsReadOnly();

  /// <summary>
  /// Private constructor for EF Core.
  /// </summary>
  private User() : base()
  {
    Email = null!;
    Password = null!;
    FullName = string.Empty;
  }

  /// <summary>
  /// Creates a new user.
  /// </summary>
  /// <param name="email">The user's email address</param>
  /// <param name="password">The user's password</param>
  /// <param name="fullName">The user's full name</param>
  /// <returns>A new User instance</returns>
  public static User Create(Email email, Password password, string fullName)
  {
    if (string.IsNullOrWhiteSpace(fullName))
    {
      throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
    }

    if (fullName.Length > 200)
    {
      throw new ArgumentException("Full name cannot exceed 200 characters.", nameof(fullName));
    }

    var user = new User
    {
      Id = Guid.NewGuid(),
      Email = email,
      Password = password,
      FullName = fullName.Trim(),
      IsEmailVerified = false,
      IsActive = true,
      FailedLoginAttempts = 0,
      LockedOutUntil = null,
      LastLoginAt = null
    };

    user.RaiseDomainEvent(new UserRegisteredEvent(
        user.Id,
        user.Email.Value,
        user.FullName));

    return user;
  }

  /// <summary>
  /// Verifies the provided password against the user's password.
  /// </summary>
  /// <param name="plainPassword">The plain text password to verify</param>
  /// <returns>True if the password is correct, false otherwise</returns>
  public bool VerifyPassword(string plainPassword)
  {
    return Password.Verify(plainPassword);
  }

  /// <summary>
  /// Changes the user's password.
  /// </summary>
  /// <param name="newPassword">The new password</param>
  public void ChangePassword(Password newPassword)
  {
    Password = newPassword ?? throw new ArgumentNullException(nameof(newPassword));
    MarkAsUpdated();
  }

  /// <summary>
  /// Updates the user's profile information.
  /// </summary>
  /// <param name="fullName">The updated full name</param>
  public void UpdateProfile(string fullName)
  {
    if (string.IsNullOrWhiteSpace(fullName))
    {
      throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
    }

    if (fullName.Length > 200)
    {
      throw new ArgumentException("Full name cannot exceed 200 characters.", nameof(fullName));
    }

    FullName = fullName.Trim();
    MarkAsUpdated();

    RaiseDomainEvent(new UserUpdatedEvent(Id, Email.Value, FullName));
  }

  /// <summary>
  /// Verifies the user's email address.
  /// </summary>
  public void VerifyEmail()
  {
    IsEmailVerified = true;
    MarkAsUpdated();
  }

  /// <summary>
  /// Deactivates the user account.
  /// </summary>
  public void Deactivate()
  {
    IsActive = false;
    MarkAsUpdated();
  }

  /// <summary>
  /// Activates the user account.
  /// </summary>
  public void Activate()
  {
    IsActive = true;
    FailedLoginAttempts = 0;
    LockedOutUntil = null;
    MarkAsUpdated();
  }

  /// <summary>
  /// Records a successful login attempt.
  /// </summary>
  /// <param name="ipAddress">The IP address of the login</param>
  /// <param name="userAgent">The user agent of the login</param>
  /// <returns>The created session</returns>
  public UserSession RecordSuccessfulLogin(string? ipAddress, string? userAgent)
  {
    if (!IsActive)
    {
      throw new InvalidOperationException("Cannot login to an inactive account.");
    }

    if (IsLockedOut())
    {
      throw new InvalidOperationException("Account is locked out. Please try again later.");
    }

    FailedLoginAttempts = 0;
    LockedOutUntil = null;
    LastLoginAt = DateTime.UtcNow;
    MarkAsUpdated();

    var session = UserSession.Create(Id, ipAddress, userAgent);
    _sessions.Add(session);

    RaiseDomainEvent(new UserSignedInEvent(
        Id,
        Email.Value,
        session.Id,
        ipAddress,
        userAgent));

    return session;
  }

  /// <summary>
  /// Records a failed login attempt and locks out the account if threshold is exceeded.
  /// </summary>
  /// <param name="maxAttempts">Maximum number of failed attempts before lockout</param>
  /// <param name="lockoutDurationMinutes">Duration of lockout in minutes</param>
  public void RecordFailedLogin(int maxAttempts = 5, int lockoutDurationMinutes = 15)
  {
    FailedLoginAttempts++;
    MarkAsUpdated();

    if (FailedLoginAttempts >= maxAttempts)
    {
      LockedOutUntil = DateTime.UtcNow.AddMinutes(lockoutDurationMinutes);
    }
  }

  /// <summary>
  /// Checks if the account is currently locked out.
  /// </summary>
  /// <returns>True if locked out, false otherwise</returns>
  public bool IsLockedOut()
  {
    if (LockedOutUntil is null)
    {
      return false;
    }

    if (LockedOutUntil.Value <= DateTime.UtcNow)
    {
      // Lockout period has expired
      LockedOutUntil = null;
      FailedLoginAttempts = 0;
      return false;
    }

    return true;
  }

  /// <summary>
  /// Terminates a specific session.
  /// </summary>
  /// <param name="sessionId">The session identifier</param>
  public void TerminateSession(Guid sessionId)
  {
    var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
    if (session is null)
    {
      throw new InvalidSessionException(sessionId);
    }

    session.Revoke();
    MarkAsUpdated();

    RaiseDomainEvent(new UserSignedOutEvent(Id, Email.Value, sessionId));
  }

  /// <summary>
  /// Terminates all active sessions for the user.
  /// </summary>
  public void TerminateAllSessions()
  {
    foreach (var session in _sessions.Where(s => s.IsActive))
    {
      session.Revoke();
    }
    MarkAsUpdated();
  }
}
