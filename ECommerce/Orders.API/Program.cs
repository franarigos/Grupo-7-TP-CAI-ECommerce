using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orders.API.ExceptionHandlers;
using Orders.API.Middlewares;
using Orders.API.Services;
using Serilog;
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
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                List<string> mensajes = new List<string>();

                foreach (ModelStateEntry entry
                    in context.ModelState.Values)
                {
                    foreach (ModelError error in entry.Errors)
                    {
                        if (!string.IsNullOrWhiteSpace(
                            error.ErrorMessage))
                        {
                            mensajes.Add(error.ErrorMessage);
                        }
                    }
                }

                string detalle = string.Join("; ", mensajes);

                if (string.IsNullOrWhiteSpace(detalle))
                {
                    detalle = "Los datos de la orden son invalidos.";
                }

                string correlationId =
                    context.HttpContext.Items[
                        "X-Correlation-Id"]?.ToString()
                    ?? context.HttpContext.TraceIdentifier;

                Log.Warning(
                    "Validacion de orden: {errorCode} {Detalle}",
                    "ORD-002",
                    detalle);

                return new BadRequestObjectResult(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Bad Request",
                    status = 400,
                    detail = "Los datos de la solicitud son invalidos.",
                    instance = context.HttpContext.Request.Path.Value,
                    errorCode = "ORD-002",
                    errorMessage = detalle,
                    correlationId = correlationId
                });
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
    builder.Services.AddScoped<IOrdersService, OrdersService>();

    builder.Services.AddHttpClient("UsersApi", client =>
    {
        string url = builder.Configuration["Services:UsersApi"];

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
        string url = builder.Configuration["Services:ProductsApi"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException(
                "Falta configurar Services:ProductsApi.");
        }

        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
    builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddProblemDetails();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

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
                        "X-Correlation-Id"]?.ToString());
            };
    });

    app.UseExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI();

    var healthOptions = new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    estado = report.Status.ToString()
                }));
        }
    };

    app.MapHealthChecks("/health", healthOptions);
    app.MapHealthChecks("/health/ready", healthOptions);
    app.MapHealthChecks("/health/live", healthOptions);

    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error al iniciar Orders.API");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
