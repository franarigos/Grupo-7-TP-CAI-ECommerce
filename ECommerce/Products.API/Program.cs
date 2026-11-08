using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.HealthChecks;
using Products.API.Middlewares;
using Products.API.Services;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;
using Serilog.Formatting.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// Configura e inicia Products.API.
/// </summary>
public class Program
{
    /// <summary>
    /// Inicia la aplicación.
    /// </summary>
    /// <param name="args">Argumentos de inicio.</param>
    public static void Main(string[] args)
    {
        string entorno =
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Production";

        IConfiguration configuracion = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings." + entorno + ".json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(
                configuracion.GetValue<LogEventLevel>(
                    "Logging:LogLevel:Default",
                    LogEventLevel.Information))
            .MinimumLevel.Override(
                "Microsoft",
                configuracion.GetValue<LogEventLevel>(
                    "Logging:LogLevel:Microsoft",
                    LogEventLevel.Warning))
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Servicio", "Products.API")
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                "{Servicio} {Endpoint} {CorrelationId} {ErrorCode} " +
                "{Message:lj}{NewLine}{Exception}")
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
                .ConfigureApiBehaviorOptions(ConfigurarValidacion);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(ConfigurarSwagger);
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddHttpClient(
                "OrdersApi",
                ConfigurarClienteOrders);

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

            builder.Services.AddProblemDetails(
                ConfigurarErroresHttp);

            builder.Services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db");

            WebApplication app = builder.Build();

            using (IServiceScope scope = app.Services.CreateScope())
            {
                DatabaseInitializer initializer =
                    scope.ServiceProvider
                        .GetRequiredService<DatabaseInitializer>();

                initializer.Initialize();
            }

            app.UseMiddleware<CorrelationIdMiddleware>();

            app.UseSerilogRequestLogging(
                ConfigurarLogSolicitudes);

            app.UseExceptionHandler();
            app.UseStatusCodePages();

            app.UseSwagger();
            app.UseSwaggerUI();

            HealthCheckOptions healthOptions =
                new HealthCheckOptions();

            healthOptions.ResponseWriter =
                EscribirEstadoSalud;

            app.MapHealthChecks(
                "/health",
                healthOptions);

            app.MapHealthChecks(
                "/health/ready",
                healthOptions);

            HealthCheckOptions liveOptions =
                new HealthCheckOptions();

            liveOptions.Predicate =
                ExcluirComprobacion;

            liveOptions.ResponseWriter =
                EscribirEstadoSalud;

            app.MapHealthChecks(
                "/health/live",
                liveOptions);

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
    }

    private static void ConfigurarValidacion(
        ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory =
            CrearErrorValidacion;
    }

    private static IActionResult CrearErrorValidacion(
        ActionContext context)
    {
        List<string> mensajes = new List<string>();

        foreach (var campo in context.ModelState.Values)
        {
            foreach (var error in campo.Errors)
            {
                if (!string.IsNullOrWhiteSpace(error.ErrorMessage))
                {
                    mensajes.Add(error.ErrorMessage);
                }
            }
        }

        string mensaje = string.Join("; ", mensajes);

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            mensaje = "Los datos del producto son inválidos.";
        }

        context.HttpContext.Items["ErrorCode"] =
            "PRD-002";

        Log.Warning(
            "Validación incorrecta {ErrorCode}: {Mensaje}",
            "PRD-002",
            mensaje);

        BadRequestObjectResult resultado =
            new BadRequestObjectResult(new
            {
                type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Los datos del producto son inválidos.",
                instance =
                    context.HttpContext.Request.Path.ToString(),
                errorCode = "PRD-002",
                errorMessage = mensaje,
                correlationId =
                    ObtenerCorrelationId(context.HttpContext)
            });

        resultado.ContentTypes.Add(
            "application/problem+json");

        return resultado;
    }

    private static void ConfigurarSwagger(
        SwaggerGenOptions options)
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
    }

    private static void ConfigurarClienteOrders(
        IServiceProvider services,
        HttpClient client)
    {
        IConfiguration config =
            services.GetRequiredService<IConfiguration>();

        string? url = config["Services:OrdersApi"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException(
                "Falta configurar Services:OrdersApi.");
        }

        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(10);
    }

    private static void ConfigurarErroresHttp(
        ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails =
            CompletarErrorHttp;
    }

    private static void CompletarErrorHttp(
        ProblemDetailsContext context)
    {
        ProblemDetails problem = context.ProblemDetails;

        int status =
            problem.Status
            ?? context.HttpContext.Response.StatusCode;

        string errorCode = "PRD-HTTP-" + status;

        string mensaje =
            "La solicitud no pudo procesarse.";

        if (status == 404)
        {
            mensaje =
                "La ruta solicitada no fue encontrada.";
        }
        else if (status == 405)
        {
            mensaje =
                "El método HTTP no está permitido para esta ruta.";
        }
        else if (status == 415)
        {
            mensaje =
                "El contenido debe enviarse en un formato admitido por la API.";
        }

        problem.Detail = mensaje;

        problem.Instance =
            context.HttpContext.Request.Path.ToString();

        problem.Extensions["errorCode"] = errorCode;
        problem.Extensions["errorMessage"] = mensaje;

        problem.Extensions["correlationId"] =
            ObtenerCorrelationId(context.HttpContext);

        context.HttpContext.Items["ErrorCode"] =
            errorCode;

        if (status >= 500)
        {
            Log.Error(
                "Error HTTP {ErrorCode}: {Mensaje}",
                errorCode,
                mensaje);
        }
        else
        {
            Log.Warning(
                "Error HTTP {ErrorCode}: {Mensaje}",
                errorCode,
                mensaje);
        }
    }

    private static string ObtenerCorrelationId(
        HttpContext context)
    {
        return context.Items["CorrelationId"]?.ToString()
            ?? context.TraceIdentifier;
    }

    private static void ConfigurarLogSolicitudes(
        RequestLoggingOptions options)
    {
        options.GetLevel = ObtenerNivelLog;

        options.EnrichDiagnosticContext =
            CompletarLogSolicitud;
    }

    private static LogEventLevel ObtenerNivelLog(
        HttpContext context,
        double elapsed,
        Exception? exception)
    {
        if (exception != null ||
            context.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        if (context.Response.StatusCode >= 400)
        {
            return LogEventLevel.Warning;
        }

        return LogEventLevel.Information;
    }

    private static void CompletarLogSolicitud(
        IDiagnosticContext diagnosticContext,
        HttpContext context)
    {
        diagnosticContext.Set(
            "Endpoint",
            context.Request.Path.ToString());

        diagnosticContext.Set(
            "CorrelationId",
            ObtenerCorrelationId(context));

        if (context.Items["ErrorCode"] != null)
        {
            diagnosticContext.Set(
                "ErrorCode",
                context.Items["ErrorCode"]!.ToString());
        }
    }

    private static bool ExcluirComprobacion(
        HealthCheckRegistration registration)
    {
        return false;
    }

    private static Task EscribirEstadoSalud(
        HttpContext context,
        HealthReport report)
    {
        return context.Response.WriteAsJsonAsync(new
        {
            estado = report.Status.ToString()
        });
    }
}