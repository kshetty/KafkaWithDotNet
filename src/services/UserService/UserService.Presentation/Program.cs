using UserService.Application;
using UserService.Infrastructure;
using UserService.Presentation.Extensions;
using UserService.Presentation.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/userservice-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
  Log.Information("Starting User Service");

  // Add services to the container
  builder.Services.AddControllers();
  builder.Services.AddEndpointsApiExplorer();

  // Add Swagger/OpenAPI
  builder.Services.AddSwaggerDocumentation();

  // Add Application and Infrastructure layers
  builder.Services.AddApplication();
  builder.Services.AddInfrastructure(builder.Configuration);

  // Add Authentication & Authorization
  builder.Services.AddAuthenticationServices(builder.Configuration);

  // Add CORS
  builder.Services.AddCorsPolicy(builder.Configuration);

  // Add Health Checks
  builder.Services.AddHealthChecks()
      .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
      .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

  var app = builder.Build();

  // Configure the HTTP request pipeline
  if (app.Environment.IsDevelopment())
  {
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API V1");
      c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
  }

  // Global exception handling middleware
  app.UseMiddleware<ExceptionHandlingMiddleware>();

  // Request logging middleware
  app.UseSerilogRequestLogging();

  app.UseHttpsRedirection();

  app.UseCors("DefaultCorsPolicy");

  app.UseAuthentication();
  app.UseAuthorization();

  app.MapControllers();
  app.MapHealthChecks("/health");

  Log.Information("User Service started successfully");
  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "User Service failed to start");
  throw;
}
finally
{
  Log.CloseAndFlush();
}

// Make the implicit Program class public for testing
public partial class Program { }
