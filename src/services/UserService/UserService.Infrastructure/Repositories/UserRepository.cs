using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

/// <summary>
/// Repository for User entity operations.
/// </summary>
public sealed class UserRepository : Repository<User, Guid>, IUserRepository
{
  public UserRepository(ApplicationDbContext context) : base(context)
  {
  }

  public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
  {
    return await Context.Users
        .FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
  }

  public async Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default)
  {
    return await Context.Users
        .AnyAsync(u => u.Email.Value == email.Value, cancellationToken);
  }

  public async Task<User?> GetWithSessionsAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await Context.Users
        .Include(u => u.Sessions)
        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
  }
}
