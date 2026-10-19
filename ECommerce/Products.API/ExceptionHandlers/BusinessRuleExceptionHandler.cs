using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Products.API.Exceptions;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Products.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();

            Log.Warning("Conflicto de negocio en {Endpoint} | Código: {ErrorCode} | CorrelationId: {CorrelationId} | Detalle: {Message}",
                context.Request.Path.Value, ex.ErrorCode, correlationId, ex.Message);

            context.Response.StatusCode = 409;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                title = "Conflict",
                status = 409,
                detail = "Ya existe un recurso con esos datos.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}