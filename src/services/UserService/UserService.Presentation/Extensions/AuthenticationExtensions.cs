using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace UserService.Presentation.Extensions;

public static class AuthenticationExtensions
{
  public static IServiceCollection AddAuthenticationServices(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var jwtSettings = configuration.GetSection("Jwt");
    var secretKey = jwtSettings["SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey is not configured");

    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
      };

      options.Events = new JwtBearerEvents
      {
        OnAuthenticationFailed = context =>
            {
              if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
              {
                context.Response.Headers.Add("Token-Expired", "true");
              }
              return Task.CompletedTask;
            },
        OnChallenge = context =>
            {
              context.HandleResponse();
              context.Response.StatusCode = StatusCodes.Status401Unauthorized;
              context.Response.ContentType = "application/json";

              var result = System.Text.Json.JsonSerializer.Serialize(new
              {
                title = "Unauthorized",
                status = 401,
                detail = "You are not authorized to access this resource"
              });

              return context.Response.WriteAsync(result);
            }
      };
    });

    services.AddAuthorization();

    return services;
  }
}
