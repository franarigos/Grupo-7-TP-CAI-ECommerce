using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public static class ErrorResponseWriter
    {
        public static async Task WriteAsync(
            HttpContext context,
            int status,
            string title,
            string errorCode,
            string message,
            CancellationToken cancellationToken)
        {
            ApiErrorResponse response = new ApiErrorResponse();

            response.Title = title;
            response.Status = status;
            response.Detail = message;
            response.Instance = context.Request.Path.ToString();
            response.ErrorCode = errorCode;
            response.ErrorMessage = message;
            response.CorrelationId = context.TraceIdentifier;

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            await System.Text.Json.JsonSerializer.SerializeAsync(
                context.Response.Body,
                response,
                new System.Text.Json.JsonSerializerOptions(
                    System.Text.Json.JsonSerializerDefaults.Web),
                cancellationToken);
        }
    }

    public class NotFoundExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<NotFoundExceptionHandler> _logger;

        public NotFoundExceptionHandler(
            ILogger<NotFoundExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not NotFoundException error)
            {
                return false;
            }

            _logger.LogWarning(
                "Recurso no encontrado. {errorCode}: {ErrorMessage}",
                error.ErrorCode,
                error.Message);

            await ErrorResponseWriter.WriteAsync(
                context,
                404,
                "Not Found",
                error.ErrorCode,
                error.Message,
                cancellationToken);

            return true;
        }
    }

    public class ValidationExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ValidationExceptionHandler> _logger;

        public ValidationExceptionHandler(
            ILogger<ValidationExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ValidationException error)
            {
                return false;
            }

            _logger.LogWarning(
                "Validación incorrecta. {errorCode}: {ErrorMessage}",
                error.ErrorCode,
                error.Message);

            await ErrorResponseWriter.WriteAsync(
                context,
                400,
                "Bad Request",
                error.ErrorCode,
                error.Message,
                cancellationToken);

            return true;
        }
    }

    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Error inesperado. {errorCode}",
                "NTF-004");

            await ErrorResponseWriter.WriteAsync(
                context,
                500,
                "Internal Server Error",
                "NTF-004",
                "Error interno al procesar la notificación.",
                cancellationToken);

            return true;
        }
    }
}
