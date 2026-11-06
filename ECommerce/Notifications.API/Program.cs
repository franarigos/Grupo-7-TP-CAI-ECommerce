using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Notifications.API.DTOs;
using Notifications.API.ExceptionHandlers;
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

    builder.Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                List<string> mensajes = new List<string>();

                foreach (ModelStateEntry? entry
                    in context.ModelState.Values)
                {
                    if (entry == null)
                    {
                        continue;
                    }

                    foreach (ModelError error in entry.Errors)
                    {
                        string mensaje = error.ErrorMessage;

                        if (string.IsNullOrWhiteSpace(mensaje))
                        {
                            mensaje = "El formato de un campo es inválido.";
                        }

                        mensajes.Add(mensaje);
                    }
                }

                string detalle = string.Join("; ", mensajes);

                ApiErrorResponse response = new ApiErrorResponse();

                response.Title = "Bad Request";
                response.Status = 400;
                response.Detail = detalle;
                response.Instance =
                    context.HttpContext.Request.Path.ToString();
                response.ErrorCode = "NTF-002";
                response.ErrorMessage = detalle;
                response.CorrelationId =
                    context.HttpContext.TraceIdentifier;

                Log.Warning(
                    "Validación automática. {errorCode}: {ErrorMessage}",
                    "NTF-002",
                    detalle);

                BadRequestObjectResult result =
                    new BadRequestObjectResult(response);

                result.ContentTypes.Add("application/problem+json");

                return result;
            };
        });

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        string xmlName =
            Assembly.GetExecutingAssembly().GetName().Name + ".xml";

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
        string? baseUrl = builder.Configuration["Services:UsersApi"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "Falta configurar Services:UsersApi.");
        }

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    builder.Services.AddScoped<NotificationService>();

    builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddProblemDetails();
    builder.Services.AddHealthChecks();

    WebApplication app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI();

    HealthCheckOptions healthOptions = new HealthCheckOptions
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

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Notifications.API terminó inesperadamente.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}