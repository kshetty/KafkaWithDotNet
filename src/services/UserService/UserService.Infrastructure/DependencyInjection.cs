using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Application.Common.Interfaces.Services;
using UserService.Infrastructure.Kafka;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Services;

namespace UserService.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
  /// <summary>
  /// Registers all Infrastructure layer services with the dependency injection container.
  /// </summary>
  /// <param name="services">The service collection</param>
  /// <param name="configuration">The application configuration</param>
  /// <returns>The service collection for chaining</returns>
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // Database
    services.AddDbContext<ApplicationDbContext>(options =>
    {
      var connectionString = configuration.GetConnectionString("PostgreSQL")
              ?? throw new InvalidOperationException("PostgreSQL connection string is not configured");

      options.UseNpgsql(connectionString, npgsqlOptions =>
          {
          npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
          npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        });

      // Enable sensitive data logging in development only
      var enableSensitiveLogging = configuration["Logging:EnableSensitiveDataLogging"];
      if (!string.IsNullOrEmpty(enableSensitiveLogging) && bool.Parse(enableSensitiveLogging))
      {
        options.EnableSensitiveDataLogging();
      }
    });

    // Repositories
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IUserSessionRepository, UserSessionRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Services
    services.AddSingleton<Application.Common.Interfaces.Services.IDateTimeProvider, DateTimeProvider>();
    services.AddSingleton<ITokenService, TokenService>();
    services.AddSingleton<ICacheService, CacheService>();
    services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

    // Redis
    services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
      var redisConnectionString = configuration.GetConnectionString("Redis")
              ?? throw new InvalidOperationException("Redis connection string is not configured");

      var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
      configurationOptions.AbortOnConnectFail = false;
      configurationOptions.ConnectRetry = 3;
      configurationOptions.ConnectTimeout = 5000;

      return ConnectionMultiplexer.Connect(configurationOptions);
    });

    // Kafka
    services.AddKafka(kafka => kafka
        .AddCluster(cluster => cluster
            .WithBrokers(new[]
            {
                    configuration["Kafka:BootstrapServers"]
                        ?? throw new InvalidOperationException("Kafka BootstrapServers is not configured")
            })
            .CreateTopicIfNotExists("user-registered", 3, 1)
            .CreateTopicIfNotExists("user-signed-in", 3, 1)
            .CreateTopicIfNotExists("user-signed-out", 3, 1)
            .CreateTopicIfNotExists("email-verified", 3, 1)
            .AddProducer(
                "user-service-producer",
                producer => producer
                    .DefaultTopic("user-registered")
                    .AddMiddlewares(middlewares => middlewares
                        .AddSerializer<JsonCoreSerializer>()
                    )
                    .WithAcks(Acks.All)
            )
        )
    );

    return services;
  }
}
