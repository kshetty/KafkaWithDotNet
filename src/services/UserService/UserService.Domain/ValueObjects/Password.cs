using System.Security.Cryptography;
using System.Text;
using UserService.Domain.Primitives;

namespace UserService.Domain.ValueObjects;

/// <summary>
/// Value object representing a password with validation and hashing.
/// Passwords are never stored in plain text.
/// </summary>
public sealed class Password : ValueObject
{
  private const int MinLength = 8;
  private const int MaxLength = 128;
  private const int SaltSize = 16; // 128 bits
  private const int HashSize = 32; // 256 bits
  private const int Iterations = 100000; // OWASP recommended minimum

  /// <summary>
  /// Gets the hashed password value.
  /// Format: {iterations}.{salt}.{hash} (Base64 encoded)
  /// </summary>
  public string HashedValue { get; }

  /// <summary>
  /// Private constructor for internal use.
  /// </summary>
  private Password(string hashedValue)
  {
    HashedValue = hashedValue;
  }

  /// <summary>
  /// Creates a new Password value object from a plain text password.
  /// The password will be hashed using PBKDF2.
  /// </summary>
  /// <param name="plainPassword">The plain text password</param>
  /// <returns>A new Password instance with hashed value</returns>
  /// <exception cref="ArgumentException">Thrown when the password doesn't meet requirements</exception>
  public static Password Create(string plainPassword)
  {
    ValidatePassword(plainPassword);

    var salt = GenerateSalt();
    var hash = HashPassword(plainPassword, salt, Iterations);
    var hashedValue = $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

    return new Password(hashedValue);
  }

  /// <summary>
  /// Creates a Password instance from an already hashed value (for loading from database).
  /// </summary>
  /// <param name="hashedValue">The hashed password value</param>
  /// <returns>A new Password instance</returns>
  public static Password FromHash(string hashedValue)
  {
    if (string.IsNullOrWhiteSpace(hashedValue))
    {
      throw new ArgumentException("Hashed value cannot be empty.", nameof(hashedValue));
    }

    return new Password(hashedValue);
  }

  /// <summary>
  /// Verifies if a plain text password matches this hashed password.
  /// </summary>
  /// <param name="plainPassword">The plain text password to verify</param>
  /// <returns>True if the password matches, false otherwise</returns>
  public bool Verify(string plainPassword)
  {
    if (string.IsNullOrWhiteSpace(plainPassword))
    {
      return false;
    }

    try
    {
      var parts = HashedValue.Split('.');
      if (parts.Length != 3)
      {
        return false;
      }

      var iterations = int.Parse(parts[0]);
      var salt = Convert.FromBase64String(parts[1]);
      var hash = Convert.FromBase64String(parts[2]);

      var testHash = HashPassword(plainPassword, salt, iterations);

      return CryptographicOperations.FixedTimeEquals(hash, testHash);
    }
    catch
    {
      return false;
    }
  }

  /// <summary>
  /// Validates password requirements.
  /// </summary>
  private static void ValidatePassword(string password)
  {
    if (string.IsNullOrWhiteSpace(password))
    {
      throw new ArgumentException("Password cannot be empty or whitespace.", nameof(password));
    }

    if (password.Length < MinLength)
    {
      throw new ArgumentException($"Password must be at least {MinLength} characters long.", nameof(password));
    }

    if (password.Length > MaxLength)
    {
      throw new ArgumentException($"Password cannot exceed {MaxLength} characters.", nameof(password));
    }

    // Check for at least one digit
    if (!password.Any(char.IsDigit))
    {
      throw new ArgumentException("Password must contain at least one digit.", nameof(password));
    }

    // Check for at least one lowercase letter
    if (!password.Any(char.IsLower))
    {
      throw new ArgumentException("Password must contain at least one lowercase letter.", nameof(password));
    }

    // Check for at least one uppercase letter
    if (!password.Any(char.IsUpper))
    {
      throw new ArgumentException("Password must contain at least one uppercase letter.", nameof(password));
    }

    // Check for at least one special character
    if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
    {
      throw new ArgumentException("Password must contain at least one special character.", nameof(password));
    }
  }

  /// <summary>
  /// Validates if a plain text password meets the requirements without creating an instance.
  /// </summary>
  /// <param name="plainPassword">The password to validate</param>
  /// <param name="errorMessage">The validation error message if invalid</param>
  /// <returns>True if valid, false otherwise</returns>
  public static bool IsValid(string plainPassword, out string? errorMessage)
  {
    try
    {
      ValidatePassword(plainPassword);
      errorMessage = null;
      return true;
    }
    catch (ArgumentException ex)
    {
      errorMessage = ex.Message;
      return false;
    }
  }

  /// <summary>
  /// Generates a cryptographically secure random salt.
  /// </summary>
  private static byte[] GenerateSalt()
  {
    var salt = new byte[SaltSize];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(salt);
    return salt;
  }

  /// <summary>
  /// Hashes a password using PBKDF2.
  /// </summary>
  private static byte[] HashPassword(string password, byte[] salt, int iterations)
  {
    using var pbkdf2 = new Rfc2898DeriveBytes(
        password,
        salt,
        iterations,
        HashAlgorithmName.SHA256);

    return pbkdf2.GetBytes(HashSize);
  }

  /// <summary>
  /// Implicit conversion from Password to string (returns hashed value).
  /// </summary>
  public static implicit operator string(Password password) => password.HashedValue;

  /// <summary>
  /// Returns the hashed password value.
  /// </summary>
  public override string ToString() => HashedValue;

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return HashedValue;
  }
}
