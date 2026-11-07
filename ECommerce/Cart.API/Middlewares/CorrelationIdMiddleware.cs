using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace Cart.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        private const string CorrelationIdHeader =
            "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId =
                context.Request.Headers[
                    CorrelationIdHeader].ToString();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Request.Headers[CorrelationIdHeader] =
                correlationId;

            context.Items["CorrelationId"] = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] =
                    correlationId;

                return Task.CompletedTask;
            });

            using (LogContext.PushProperty(
                "CorrelationId",
                correlationId))
            using (LogContext.PushProperty(
                "Endpoint",
                context.Request.Path.ToString()))
            {
                Log.Information(
                    "Inicio de solicitud {Metodo} {Endpoint}",
                    context.Request.Method,
                    context.Request.Path.ToString());

                await _next(context);
            }
        }
    }
}