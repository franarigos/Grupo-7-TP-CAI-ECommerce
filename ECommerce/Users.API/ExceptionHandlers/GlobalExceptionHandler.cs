using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Users.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error inesperado al procesar la solicitud.",
                instance = context.Request.Path.Value,
                errorCode = "USR-006",
                errorMessage = "Error interno al procesar el usuario.",
                correlationId = correlationId
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}