namespace UserService.Presentation.Extensions;

public static class CorsExtensions
{
  public static IServiceCollection AddCorsPolicy(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000" };

    services.AddCors(options =>
    {
      options.AddPolicy("DefaultCorsPolicy", policy =>
          {
          policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
    });

    return services;
  }
}
