
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.ExceptionHandlers;
using Orders.API.HealthChecks;
using Orders.API.Middlewares;
using Orders.API.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Orders.API")
    .WriteTo.Console()
    .WriteTo.File(
        new JsonFormatter(),
        "logs/orders-api-.json",
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
                    mensaje = "Los datos de la orden son inválidos.";
                }

                string correlationId =
                    context.HttpContext.Items[
                        "X-Correlation-Id"]?.ToString()
                    ?? context.HttpContext.TraceIdentifier;

                Log.Warning(
                    "Validación incorrecta {ErrorCode}: {Mensaje}",
                    "ORD-002",
                    mensaje);

                ApiErrorResponse errorResponse =
                    new ApiErrorResponse
                    {
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Title = "Bad Request",
                        Status = 400,
                        Detail = "Los datos de la orden son inválidos.",
                        Instance =
                            context.HttpContext.Request.Path.ToString(),
                        ErrorCode = "ORD-002",
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

    builder.Services.AddHttpClient("UsersApi", client =>
    {
        string url =
            builder.Configuration["Services:UsersApi"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException(
                "Falta configurar Services:UsersApi.");
        }

        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    builder.Services.AddHttpClient("ProductsApi", client =>
    {
        string url =
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

    builder.Services.AddScoped<OrderRepository>();

    builder.Services.AddScoped<
        IOrdersService, OrdersService>();

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
                        "X-Correlation-Id"]?.ToString());
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
        "Error al iniciar Orders.API");

    throw;
}
finally
{
    Log.CloseAndFlush();
}
