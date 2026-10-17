using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Formatting.Json;
using System;
using System.Net.Http;
using System.Text.Json;
using Cart.API.ExceptionHandlers;
using Cart.API.Services;
using Cart.API.Middlewares;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Cart.API")
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "logs/cart-api-log.json", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
    builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddHealthChecks();

    builder.Services.AddHttpClient("ProductsAPI", (provider, client) =>
    {
        client.BaseAddress = new Uri("https://localhost:44316");
        var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
        if (httpContextAccessor.HttpContext != null &&
            httpContextAccessor.HttpContext.Items.TryGetValue("CorrelationId", out var correlationId))
        {
            client.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId.ToString());
        }
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    });

    builder.Services.AddScoped<CartService>();

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("CorrelationId", httpContext.Items["CorrelationId"]);
            diagnosticContext.Set("Endpoint", httpContext.Request.Path);
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseExceptionHandler();

    var healthCheckOptions = new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new { estado = report.Status.ToString() };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    };

    app.MapHealthChecks("/health", healthCheckOptions);
    app.MapHealthChecks("/health/ready", healthCheckOptions);
    app.MapHealthChecks("/health/live", healthCheckOptions);

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "El microservicio Cart.API se cerro inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}