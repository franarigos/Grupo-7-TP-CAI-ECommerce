using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString()
                                ?? context.Request.Headers["X-Correlation-Id"].ToString();

            Log.Error(exception, "Error inesperado en {Endpoint} | Código: CRT-005 | CorrelationId: {CorrelationId} | Detalle: {Message}",
                context.Request.Path.Value, correlationId, exception.Message);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error interno en el servidor.",
                instance = context.Request.Path.Value,
                errorCode = "CRT-005",
                errorMessage = "Error interno al procesar el carrito.",
                correlationId = correlationId
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}