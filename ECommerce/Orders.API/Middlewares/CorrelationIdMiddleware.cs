using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace Orders.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        private const string HeaderName =
            "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId =
                context.Request.Headers[HeaderName].ToString();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Request.Headers[HeaderName] =
                correlationId;

            context.Items[HeaderName] = correlationId;

            context.Response.OnStarting(
                AgregarCorrelationId,
                context);

            using (LogContext.PushProperty(
                "CorrelationId", correlationId))
            using (LogContext.PushProperty(
                "Endpoint", context.Request.Path.ToString()))
            {
                Log.Information(
                    "Inicio de solicitud {Metodo} {Endpoint}",
                    context.Request.Method,
                    context.Request.Path.ToString());

                await _next(context);
            }
        }

        private static Task AgregarCorrelationId(object estado)
        {
            HttpContext context = (HttpContext)estado;

            context.Response.Headers["X-Correlation-Id"] =
                context.Items["X-Correlation-Id"]?.ToString()
                ?? context.TraceIdentifier;

            return Task.CompletedTask;
        }
    }
}