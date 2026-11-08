using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Notifications.API.Middlewares
{
    /// <summary>
    /// Identifica las solicitudes y registra su inicio y finalización.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        /// <summary>
        /// Inicializa el middleware de correlación.
        /// </summary>
        /// <param name="next">Siguiente componente de la solicitud.</param>
        /// <param name="logger">Registro de eventos.</param>
        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Asigna la correlación y registra la duración de la solicitud.
        /// </summary>
        /// <param name="context">Contexto de la solicitud.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId =
                context.Request.Headers["X-Correlation-Id"].ToString();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.TraceIdentifier = correlationId;
            context.Items["CorrelationId"] = correlationId;

            context.Response.OnStarting(
                AgregarCorrelationId,
                context);

            using (LogContext.PushProperty(
                "CorrelationId",
                correlationId))
            using (LogContext.PushProperty(
                "Endpoint",
                context.Request.Path.ToString()))
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

                    LogLevel level = LogLevel.Information;

                    if (context.Response.StatusCode >= 500)
                    {
                        level = LogLevel.Error;
                    }
                    else if (context.Response.StatusCode >= 400)
                    {
                        level = LogLevel.Warning;
                    }

                    _logger.Log(
                        level,
                        "Fin de request. HTTP {StatusCode}. Duración {DurationMs} ms. Código {ErrorCode}",
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds,
                        context.Items["ErrorCode"]?.ToString());
                }
            }
        }

        private static Task AgregarCorrelationId(object estado)
        {
            HttpContext context = (HttpContext)estado;

            context.Response.Headers["X-Correlation-Id"] =
                context.Items["CorrelationId"]?.ToString()
                ?? context.TraceIdentifier;

            return Task.CompletedTask;
        }
    }
}