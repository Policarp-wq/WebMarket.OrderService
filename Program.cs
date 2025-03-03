using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using WebMarket.OrderService.AppExtensions;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Options;
using WebMarket.OrderService.SupportTools.MapSupport;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
    .WriteTo.Console()
    .CreateLogger();
builder.Services.AddSerilog();
builder.Services.RegisterKafkaProducer(builder.Configuration.GetSection("Kafka"));
//Background service
//builder.Services.AddHostedService<KafkaConsumer>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
//extract into the new extension
builder.Services.AddEndpointsApiExplorer();
builder.Services.UseDependencyInjection();
builder.Services.AddSwaggerGen();

var healthCheckBuilder = builder.Services
   .AddHealthChecks();

builder.Services.ConfigureOptions<DatabaseOptionsSetup>();
builder.Services.ConnectDb(healthCheckBuilder);

builder.Services.ConfigureOptions<RedisOptionsSetup>();
builder.Services.ConfigureRedis(healthCheckBuilder);



builder.Services.RegisterHttpClient(builder.Configuration.GetValue<string>(YandexAPI.YandexGeoAPIKeyConfigName));


var app = builder.Build();

app.UseSerilogRequestLogging(opt =>
{
});
app.UseHealthChecks("/healtz");
app.AddSwagger(app.Environment.IsDevelopment());
app.UseExceptionHandler();
app.AddEndpoints();

var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();
var report = await healthCheckService.CheckHealthAsync();
if (report.Status != HealthStatus.Healthy)
{
    Log.Fatal("Health check failed at startup: {Status}", report.Status);
    foreach (var entry in report.Entries)
    {
        Log.Fatal("{Key}: {Value}", entry.Key, entry.Value.Status);
    }
    return;
}
else
{
    Log.Information("Service is healthy");
}

app.UseHttpsRedirection();
app.Run();
