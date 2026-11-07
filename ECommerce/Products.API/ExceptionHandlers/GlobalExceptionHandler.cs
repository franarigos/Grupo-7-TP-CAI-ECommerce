using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Products.API.ExceptionHandlers
{
    /// <summary>
    /// Maneja los errores inesperados de Products.API.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Registra el error y devuelve una respuesta con el código PRD-005.
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
            const string errorCode = "PRD-005";

            string correlationId =
                context.Items["CorrelationId"]?.ToString()
                ?? context.TraceIdentifier;

            context.Items["ErrorCode"] = errorCode;

            Log.Error(
                exception,
                "Error inesperado {ErrorCode} en {Endpoint}",
                errorCode,
                context.Request.Path.ToString());

            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    type =
                        "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    title = "Internal Server Error",
                    status = 500,
                    detail =
                        "Ocurrió un error inesperado en el servidor.",
                    instance = context.Request.Path.ToString(),
                    errorCode,
                    errorMessage =
                        "Error interno al procesar el producto.",
                    correlationId
                },
                options: null,
                contentType: "application/problem+json",
                cancellationToken: cancellationToken);

            return true;
        }
    }
}