using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Orders.API.Exceptions;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orders.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex)
            {
                return false;
            }

            int statusCode = 409;
            string titulo = "Conflict";
            string detalle = "No se puede modificar el estado.";
            string tipo =
                "https://tools.ietf.org/html/rfc7231#section-6.5.9";

            if (ex.ErrorCode == ErrorCodes.ORD_005)
            {
                statusCode = 422;
                titulo = "Unprocessable Entity";
                detalle = "No se puede procesar la solicitud.";
                tipo =
                    "https://tools.ietf.org/html/rfc4918#section-11.2";
            }

            Log.Warning(
                "Error de negocio {errorCode}: {Mensaje}",
                ex.ErrorCode,
                ex.Message);

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new
            {
                type = tipo,
                title = titulo,
                status = statusCode,
                detail = detalle,
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
