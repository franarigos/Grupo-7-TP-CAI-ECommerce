using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Cart.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.TryAdd(CorrelationIdHeader, correlationId);
            }

            context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

            context.Items["CorrelationId"] = correlationId.ToString();

            await _next(context);
        }
    }
}