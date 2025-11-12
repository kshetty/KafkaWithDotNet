using MediatR;
using UserService.Application.Common.Interfaces.Persistence;

namespace UserService.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for managing database transactions.
/// Wraps command handlers in a transaction and commits if successful.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
  private readonly IUnitOfWork _unitOfWork;

  public TransactionBehavior(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<TResponse> Handle(
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken)
  {
    // Only use transactions for commands (not queries)
    var requestName = typeof(TRequest).Name;
    if (requestName.EndsWith("Query"))
    {
      return await next();
    }

    // Execute within a transaction
    await _unitOfWork.BeginTransactionAsync(cancellationToken);

    try
    {
      var response = await next();
      await _unitOfWork.CommitTransactionAsync(cancellationToken);
      return response;
    }
    catch
    {
      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
      throw;
    }
  }
}
