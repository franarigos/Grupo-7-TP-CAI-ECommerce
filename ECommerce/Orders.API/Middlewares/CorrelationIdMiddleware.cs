using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace Orders.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        private const string HeaderName = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = "";

            if (context.Request.Headers.TryGetValue(
                HeaderName, out var valor))
            {
                correlationId = valor.ToString();
            }

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Request.Headers[HeaderName] = correlationId;
            context.Items[HeaderName] = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] =
                    correlationId;

                return Task.CompletedTask;
            });

            using (LogContext.PushProperty(
                "CorrelationId", correlationId))
            using (LogContext.PushProperty(
                "Endpoint", context.Request.Path.ToString()))
            {
                await _next(context);
            }
        }
    }
}