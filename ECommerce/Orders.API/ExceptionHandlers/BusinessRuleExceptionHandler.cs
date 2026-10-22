using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Orders.API.Exceptions;
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
            if (exception is not BusinessRuleException ex) return false;

            // Diferenciamos si es error de stock (422) o de estado inválido (409)
            int statusCode = ex.ErrorCode == ErrorCodes.ORD_005 ? StatusCodes.Status422UnprocessableEntity : StatusCodes.Status409Conflict;

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                type = statusCode == StatusCodes.Status422UnprocessableEntity ? "https://tools.ietf.org/html/rfc4918#section-11.2" : "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                title = statusCode == StatusCodes.Status422UnprocessableEntity ? "Unprocessable Entity" : "Conflict",
                status = statusCode,
                detail = "La operación no puede procesarse debido a una regla de negocio.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
