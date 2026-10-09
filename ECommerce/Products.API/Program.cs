using Serilog;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "logs/products-api-.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddScoped<Products.API.Services.ProductService>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Registro de manejadores de errores de negocio
    builder.Services.AddExceptionHandler<Products.API.ExceptionHandlers.NotFoundExceptionHandler>();
    builder.Services.AddExceptionHandler<Products.API.ExceptionHandlers.ValidationExceptionHandler>();
    builder.Services.AddExceptionHandler<Products.API.ExceptionHandlers.BusinessRuleExceptionHandler>();
    builder.Services.AddExceptionHandler<Products.API.ExceptionHandlers.GlobalExceptionHandler>();

    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
finally
{
    Log.CloseAndFlush();
}