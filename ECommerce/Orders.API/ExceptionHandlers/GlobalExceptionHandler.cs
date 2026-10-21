using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Orders.API.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orders.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = StatusCodes.Status500InternalServerError,
                detail = "Ocurrió un error inesperado en el servidor.",
                instance = context.Request.Path.Value,
                errorCode = ErrorCodes.ORD_007,
                errorMessage = "Error interno al procesar la orden."
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
