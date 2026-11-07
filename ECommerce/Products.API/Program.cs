
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.HealthChecks;
using Products.API.Middlewares;
using Products.API.Services;
using Serilog;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Products.API")
    .WriteTo.Console()
    .WriteTo.File(
        new JsonFormatter(),
        "logs/products-api-.json",
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
                    mensaje =
                        "Los datos del producto son inválidos.";
                }

                string correlationId =
                    context.HttpContext.Items[
                        "CorrelationId"]?.ToString()
                    ?? context.HttpContext.TraceIdentifier;

                Log.Warning(
                    "Validación incorrecta {ErrorCode}: {Mensaje}",
                    "PRD-002",
                    mensaje);

                BadRequestObjectResult resultado =
                    new BadRequestObjectResult(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "Los datos del producto son inválidos.",
                        instance = context.HttpContext.Request.Path.Value,
                        errorCode = "PRD-002",
                        errorMessage = mensaje,
                        correlationId = correlationId
                    });

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

    builder.Services.AddHttpClient("OrdersApi", client =>
    {
        string? url =
            builder.Configuration["Services:OrdersApi"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException(
                "Falta configurar Services:OrdersApi.");
        }

        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    builder.Services.AddSingleton<DatabaseInitializer>();
    builder.Services.AddScoped<ProductRepository>();
    builder.Services.AddScoped<ProductService>();

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
        "Error inesperado al iniciar Products.API");

    throw;
}
finally
{
    Log.CloseAndFlush();
}
