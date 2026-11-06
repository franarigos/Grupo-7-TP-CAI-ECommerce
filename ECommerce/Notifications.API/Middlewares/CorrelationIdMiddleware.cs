using System.Diagnostics;
using Serilog.Context;

namespace Notifications.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string receivedId =
                context.Request.Headers["X-Correlation-Id"].ToString();

            string correlationId;

            if (Guid.TryParse(receivedId, out Guid validId))
            {
                correlationId = validId.ToString();
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.TraceIdentifier = correlationId;
            context.Response.Headers["X-Correlation-Id"] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty(
                "Endpoint", context.Request.Path.ToString()))
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                _logger.LogInformation(
                    "Inicio de request {Method}",
                    context.Request.Method);

                try
                {
                    await _next(context);
                }
                finally
                {
                    stopwatch.Stop();

                    _logger.LogInformation(
                        "Fin de request. HTTP {StatusCode}. Duración {DurationMs} ms",
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}
