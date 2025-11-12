using Microsoft.EntityFrameworkCore.Storage;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation coordinating repository operations and transactions.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
  private readonly ApplicationDbContext _context;
  private IDbContextTransaction? _currentTransaction;

  public UnitOfWork(
      ApplicationDbContext context,
      IUserRepository users,
      IUserSessionRepository userSessions)
  {
    _context = context;
    Users = users;
    UserSessions = userSessions;
  }

  public IUserRepository Users { get; }

  public IUserSessionRepository UserSessions { get; }

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    return await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
  {
    if (_currentTransaction != null)
    {
      return; // Transaction already started
    }

    _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
  }

  public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _context.SaveChangesAsync(cancellationToken);

      if (_currentTransaction != null)
      {
        await _currentTransaction.CommitAsync(cancellationToken);
      }
    }
    catch
    {
      await RollbackTransactionAsync(cancellationToken);
      throw;
    }
    finally
    {
      if (_currentTransaction != null)
      {
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
      }
    }
  }

  public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      if (_currentTransaction != null)
      {
        await _currentTransaction.RollbackAsync(cancellationToken);
      }
    }
    finally
    {
      if (_currentTransaction != null)
      {
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
      }
    }
  }

  public void Dispose()
  {
    _currentTransaction?.Dispose();
    _context.Dispose();
  }
}
