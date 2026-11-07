
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
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
            response.Instance = context.Request.Path.ToString();
            response.ErrorCode = errorCode;
            response.ErrorMessage = message;
            response.CorrelationId = context.TraceIdentifier;

            if (status == 400)
            {
                response.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1";

                response.Detail =
                    "Los datos enviados no son válidos.";
            }
            else if (status == 404)
            {
                response.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4";

                response.Detail =
                    "El recurso solicitado no fue encontrado.";
            }
            else
            {
                response.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.6.1";

                response.Detail =
                    "Ocurrió un error inesperado.";
            }

            context.Items["ErrorCode"] = errorCode;

            context.Response.StatusCode = status;
            context.Response.ContentType =
                "application/problem+json";

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                response,
                new JsonSerializerOptions(
                    JsonSerializerDefaults.Web),
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
                "Recurso no encontrado {ErrorCode}: {Mensaje}",
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
                "Validación incorrecta {ErrorCode}: {Mensaje}",
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
                "Error inesperado {ErrorCode}",
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
