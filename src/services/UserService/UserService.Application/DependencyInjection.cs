using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using UserService.Application.Common.Behaviors;

namespace UserService.Application;

/// <summary>
/// Extension methods for configuring Application layer services.
/// </summary>
public static class DependencyInjection
{
  /// <summary>
  /// Registers all Application layer services with the dependency injection container.
  /// </summary>
  /// <param name="services">The service collection</param>
  /// <returns>The service collection for chaining</returns>
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    var assembly = Assembly.GetExecutingAssembly();

    // Register MediatR
    services.AddMediatR(config =>
    {
      config.RegisterServicesFromAssembly(assembly);
    });

    // Register FluentValidation validators
    services.AddValidatorsFromAssembly(assembly);

    // Register pipeline behaviors (order matters - they run in the order registered)
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

    return services;
  }
}
