using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using global::Cart.API.Data;
using global::Cart.API.DTOs;
using global::Cart.API.ExceptionHandlers;
using global::Cart.API.HealthChecks;
using global::Cart.API.Middlewares;
using global::Cart.API.Services;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Cart.API")
    .WriteTo.Console()
    .WriteTo.File(
        new JsonFormatter(),
        "logs/cart-api-.json",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    WebApplicationBuilder builder =
        WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                List<string> mensajes = new List<string>();

                foreach (var entry in context.ModelState.Values)
                {
                    foreach (var error in entry.Errors)
                    {
                        if (!string.IsNullOrWhiteSpace(
                            error.ErrorMessage))
                        {
                            mensajes.Add(error.ErrorMessage);
                        }
                    }
                }

                string mensaje = string.Join("; ", mensajes);

                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    mensaje = "Cantidad inválida.";
                }

                string correlationId =
                    context.HttpContext.Items[
                        "CorrelationId"]?.ToString()
                    ?? context.HttpContext.TraceIdentifier;

                context.HttpContext.Items["ErrorCode"] =
                    "CRT-004";

                Log.Warning(
                    "Error de validación {ErrorCode}: {Mensaje}",
                    "CRT-004",
                    mensaje);

                ApiErrorResponse errorResponse =
                    new ApiErrorResponse
                    {
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Title = "Bad Request",
                        Status = 400,
                        Detail = "Los datos enviados no son válidos.",
                        Instance =
                            context.HttpContext.Request.Path.ToString(),
                        ErrorCode = "CRT-004",
                        ErrorMessage = mensaje,
                        CorrelationId = correlationId
                    };

                BadRequestObjectResult resultado =
                    new BadRequestObjectResult(errorResponse);

                resultado.ContentTypes.Add(
                    "application/problem+json");

                return resultado;
            };
        });

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        string xmlFile =
            Assembly.GetExecutingAssembly().GetName().Name
            + ".xml";

        string xmlPath =
            Path.Combine(AppContext.BaseDirectory, xmlFile);

        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddHttpClient("ProductsAPI", client =>
    {
        string? url =
            builder.Configuration["Services:ProductsApi"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException(
                "Falta configurar Services:ProductsApi.");
        }

        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    builder.Services.AddSingleton<DatabaseInitializer>();
    builder.Services.AddScoped<CartRepository>();
    builder.Services.AddScoped<CartService>();

    builder.Services.AddExceptionHandler<
        NotFoundExceptionHandler>();

    builder.Services.AddExceptionHandler<
        BusinessRuleExceptionHandler>();

    builder.Services.AddExceptionHandler<
        ValidationExceptionHandler>();

    builder.Services.AddExceptionHandler<
        GlobalExceptionHandler>();

    builder.Services.AddProblemDetails();

    builder.Services.AddHealthChecks()
        .AddCheck<SqliteHealthCheck>("sqlite-db");

    WebApplication app = builder.Build();

    using (IServiceScope scope =
        app.Services.CreateScope())
    {
        DatabaseInitializer initializer =
            scope.ServiceProvider
                .GetRequiredService<DatabaseInitializer>();

        initializer.Initialize();
    }

    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel =
            (httpContext, elapsed, exception) =>
            {
                if (exception != null ||
                    httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode >= 400)
                {
                    return LogEventLevel.Warning;
                }

                return LogEventLevel.Information;
            };

        options.EnrichDiagnosticContext =
            (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set(
                    "Endpoint",
                    httpContext.Request.Path.ToString());

                diagnosticContext.Set(
                    "CorrelationId",
                    httpContext.Items[
                        "CorrelationId"]?.ToString());

                if (httpContext.Items["ErrorCode"] != null)
                {
                    diagnosticContext.Set(
                        "ErrorCode",
                        httpContext.Items[
                            "ErrorCode"]?.ToString());
                }
            };
    });

    app.UseExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI();

    HealthCheckOptions healthOptions =
        new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType =
                    "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        estado = report.Status.ToString()
                    }));
            }
        };

    app.MapHealthChecks("/health", healthOptions);

    app.MapHealthChecks("/health/ready", healthOptions);

    HealthCheckOptions liveOptions =
        new HealthCheckOptions
        {
            Predicate = registration => false,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType =
                    "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        estado = report.Status.ToString()
                    }));
            }
        };

    app.MapHealthChecks("/health/live", liveOptions);

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(
        ex,
        "Error inesperado al iniciar Cart.API");

    throw;
}
finally
{
    Log.CloseAndFlush();
}