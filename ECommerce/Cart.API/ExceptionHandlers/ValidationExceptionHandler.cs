using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Cart.API.Exceptions;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.API.ExceptionHandlers
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ValidationException ex) return false;

            var correlationId = context.Items["CorrelationId"]?.ToString()
                                ?? context.Request.Headers["X-Correlation-Id"].ToString();

            Log.Warning("Error de validación en {Endpoint} | Código: {ErrorCode} | CorrelationId: {CorrelationId} | Detalle: {Message}",
                context.Request.Path.Value, ex.ErrorCode, correlationId, ex.Message);

            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Los datos enviados no son válidos.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}