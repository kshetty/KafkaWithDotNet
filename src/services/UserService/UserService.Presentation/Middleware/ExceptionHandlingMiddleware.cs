using System.Net;
using System.Text.Json;
using UserService.Domain.Exceptions;
using FluentValidation;

namespace UserService.Presentation.Middleware;

public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;

  public ExceptionHandlingMiddleware(
      RequestDelegate next,
      ILogger<ExceptionHandlingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
      await HandleExceptionAsync(context, ex);
    }
  }

  private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    var (statusCode, errorResponse) = exception switch
    {
      ValidationException validationException => (
          HttpStatusCode.BadRequest,
          new ErrorResponse
          {
            Title = "Validation Error",
            Status = (int)HttpStatusCode.BadRequest,
            Errors = validationException.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(
                      g => g.Key,
                      g => g.Select(e => e.ErrorMessage).ToArray())
          }),

      DomainException domainException => (
          HttpStatusCode.BadRequest,
          new ErrorResponse
          {
            Title = "Domain Error",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = domainException.Message
          }),

      UnauthorizedAccessException => (
          HttpStatusCode.Unauthorized,
          new ErrorResponse
          {
            Title = "Unauthorized",
            Status = (int)HttpStatusCode.Unauthorized,
            Detail = "You are not authorized to access this resource"
          }),

      KeyNotFoundException => (
          HttpStatusCode.NotFound,
          new ErrorResponse
          {
            Title = "Not Found",
            Status = (int)HttpStatusCode.NotFound,
            Detail = exception.Message
          }),

      _ => (
          HttpStatusCode.InternalServerError,
          new ErrorResponse
          {
            Title = "Internal Server Error",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = "An unexpected error occurred. Please try again later."
          })
    };

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)statusCode;

    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    var json = JsonSerializer.Serialize(errorResponse, options);
    await context.Response.WriteAsync(json);
  }

  private class ErrorResponse
  {
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Detail { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
  }
}
