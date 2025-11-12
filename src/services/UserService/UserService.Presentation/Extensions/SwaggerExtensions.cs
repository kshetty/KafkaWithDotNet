using Microsoft.OpenApi.Models;

namespace UserService.Presentation.Extensions;

public static class SwaggerExtensions
{
  public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
  {
    services.AddSwaggerGen(options =>
    {
      options.SwaggerDoc("v1", new OpenApiInfo
      {
        Title = "User Service API",
        Version = "v1",
        Description = "User authentication and management service using Clean Architecture with CQRS",
        Contact = new OpenApiContact
        {
          Name = "User Service Team",
          Email = "support@userservice.com"
        }
      });

      // Add JWT Authentication to Swagger
      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
      });

      options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
        });

      // Enable annotations
      options.EnableAnnotations();
    });

    return services;
  }
}
