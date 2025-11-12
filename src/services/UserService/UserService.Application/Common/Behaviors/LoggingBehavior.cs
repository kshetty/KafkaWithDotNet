using MediatR;
using Microsoft.Extensions.Logging;

namespace UserService.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for logging requests and responses.
/// Useful for debugging and auditing.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
  private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

  public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
  {
    _logger = logger;
  }

  public async Task<TResponse> Handle(
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken)
  {
    var requestName = typeof(TRequest).Name;

    _logger.LogInformation(
        "Handling {RequestName}: {@Request}",
        requestName,
        request);

    try
    {
      var response = await next();

      _logger.LogInformation(
          "Handled {RequestName} successfully",
          requestName);

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Error handling {RequestName}: {ErrorMessage}",
          requestName,
          ex.Message);

      throw;
    }
  }
}
