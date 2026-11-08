using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Cart.API.ExceptionHandlers
{
    /// <summary>
    /// Maneja los errores inesperados del carrito.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Registra el error y devuelve el código CRT-005.
        /// </summary>
        /// <param name="context">Contexto de la solicitud.</param>
        /// <param name="exception">Excepción no contemplada.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>
        /// Verdadero cuando la excepción fue procesada.
        /// </returns>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            string correlationId =
                context.Items["CorrelationId"]?.ToString()
                ?? context.Request.Headers[
                    "X-Correlation-Id"].ToString();

            context.Items["ErrorCode"] = "CRT-005";

            Log.Error(
                exception,
                "Error inesperado {ErrorCode} en {Endpoint}",
                "CRT-005",
                context.Request.Path.ToString());

            context.Response.StatusCode = 500;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    type =
                        "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    title = "Internal Server Error",
                    status = 500,
                    detail =
                        "Ocurrió un error interno en el servidor.",
                    instance = context.Request.Path.Value,
                    errorCode = "CRT-005",
                    errorMessage =
                        "Error interno al procesar el carrito.",
                    correlationId
                },
                cancellationToken: cancellationToken);

            return true;
        }
    }
}