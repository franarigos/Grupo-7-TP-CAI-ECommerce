
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.ExceptionHandlers;
using Notifications.API.HealthChecks;
using Notifications.API.Middlewares;
using Notifications.API.Services;
using Serilog;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Notifications.API")
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
        "{Servicio} {Endpoint} {CorrelationId} " +
        "{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        new JsonFormatter(),
        "logs/notifications-api-.json",
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
                        string mensaje = error.ErrorMessage;

                        if (string.IsNullOrWhiteSpace(mensaje))
                        {
                            mensaje =
                                "El formato de un campo es inválido.";
                        }

                        mensajes.Add(mensaje);
                    }
                }

                string detalle = string.Join("; ", mensajes);

                if (string.IsNullOrWhiteSpace(detalle))
                {
                    detalle =
                        "Los datos de la notificación son inválidos.";
                }

                ApiErrorResponse errorResponse =
                    new ApiErrorResponse();

                errorResponse.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1";

                errorResponse.Title = "Bad Request";
                errorResponse.Status = 400;
                errorResponse.Detail =
                    "Los datos enviados no son válidos.";

                errorResponse.Instance =
                    context.HttpContext.Request.Path.ToString();

                errorResponse.ErrorCode = "NTF-002";
                errorResponse.ErrorMessage = detalle;

                errorResponse.CorrelationId =
                    context.HttpContext.TraceIdentifier;

                context.HttpContext.Items["ErrorCode"] =
                    "NTF-002";

                Log.Warning(
                    "Validación automática {ErrorCode}: {Mensaje}",
                    "NTF-002",
                    detalle);

                BadRequestObjectResult result =
                    new BadRequestObjectResult(errorResponse);

                result.ContentTypes.Add(
                    "application/problem+json");

                return result;
            };
        });

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        string xmlName =
            Assembly.GetExecutingAssembly().GetName().Name
            + ".xml";

        string xmlPath =
            Path.Combine(AppContext.BaseDirectory, xmlName);

        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddHttpClient("UsersAPI", client =>
    {
        string? baseUrl =
            builder.Configuration["Services:UsersApi"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "Falta configurar Services:UsersApi.");
        }

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    builder.Services.AddSingleton<DatabaseInitializer>();
    builder.Services.AddScoped<NotificationRepository>();
    builder.Services.AddScoped<NotificationService>();

    builder.Services.AddExceptionHandler<
        NotFoundExceptionHandler>();

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

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(
        ex,
        "Error al iniciar Notifications.API");

    throw;
}
finally
{
    Log.CloseAndFlush();
}
