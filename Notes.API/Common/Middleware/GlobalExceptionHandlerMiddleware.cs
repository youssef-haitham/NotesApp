using NotesApp.API.Common.Dtos;
using NotesApp.API.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace NotesApp.API.Common.Middleware
{
    public class GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new ErrorResponseDto
            {
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case BaseException baseException:
                    response.StatusCode = baseException.StatusCode;
                    errorResponse.ErrorCode = baseException.ErrorCode ?? "ERROR";
                    errorResponse.Message = baseException.Message;

                    if (exception is ValidationException validationException && validationException.Errors.Any())
                    {
                        errorResponse.Errors = validationException.Errors;
                    }
                    break;

                case BadHttpRequestException badRequest:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = "BAD_REQUEST";
                    errorResponse.Message = badRequest.Message;
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.ErrorCode = "UNAUTHORIZED";
                    errorResponse.Message = "Unauthorized access";
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.ErrorCode = "NOT_FOUND";
                    errorResponse.Message = "The requested resource was not found";
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.ErrorCode = "INTERNAL_SERVER_ERROR";
                    errorResponse.Message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "An error occurred while processing your request";
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(errorResponse, JsonOptions);
            await response.WriteAsync(jsonResponse);
        }
    }
}
