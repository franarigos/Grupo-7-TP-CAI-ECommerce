using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Products.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();

            Log.Error(exception, "Error inesperado en {Endpoint} | Código: PRD-005 | CorrelationId: {CorrelationId} | Detalle: {Message}",
                context.Request.Path.Value, correlationId, exception.Message);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error inesperado en el servidor.",
                instance = context.Request.Path.Value,
                errorCode = "PRD-005",
                errorMessage = "Error interno al procesar el producto.",
                correlationId = correlationId
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}