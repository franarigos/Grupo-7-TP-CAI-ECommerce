using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Orders.API.Exceptions;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orders.API.ExceptionHandlers
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ValidationException ex)
            {
                return false;
            }

            context.Items["ErrorCode"] = ex.ErrorCode;

            Log.Warning(
                "Error de validacion {ErrorCode}: {Mensaje}",
                ex.ErrorCode,
                ex.Message);

            context.Response.StatusCode = 400;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Los datos de la solicitud son invalidos.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId =
                    context.Items["X-Correlation-Id"]?.ToString()
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}