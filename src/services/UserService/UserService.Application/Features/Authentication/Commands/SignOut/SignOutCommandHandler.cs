using MediatR;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Application.Common.Interfaces.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Authentication.Commands.SignOut;

/// <summary>
/// Handler for SignOutCommand.
/// </summary>
public sealed class SignOutCommandHandler : IRequestHandler<SignOutCommand, Unit>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IEventPublisher _eventPublisher;

  public SignOutCommandHandler(
      IUnitOfWork unitOfWork,
      IEventPublisher eventPublisher)
  {
    _unitOfWork = unitOfWork;
    _eventPublisher = eventPublisher;
  }

  public async Task<Unit> Handle(SignOutCommand request, CancellationToken cancellationToken)
  {
    // Get user with sessions
    var user = await _unitOfWork.Users.GetWithSessionsAsync(request.UserId, cancellationToken)
        ?? throw new UserNotFoundException(request.UserId);

    // Terminate the session
    user.TerminateSession(request.SessionId);

    // Save changes
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // Publish domain events to Kafka
    await _eventPublisher.PublishManyAsync(user.DomainEvents, cancellationToken);

    return Unit.Value;
  }
}
