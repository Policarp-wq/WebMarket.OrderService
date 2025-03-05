using GeoJSON.Net.Converters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;
using WebMarket.OrderService.AppExtensions;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Options;
using WebMarket.OrderService.SupportTools.MapSupport;

var builder = WebApplication.CreateBuilder(args);
bool isDevelopment = builder.Environment.IsDevelopment();

builder.AddLogging();
builder.Services.RegisterKafkaProducer(builder.Configuration.GetSection("Kafka"));
//Background service
//builder.Services.AddHostedService<KafkaConsumer>();
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
//extract into the new extension
builder.Services.AddEndpointsApiExplorer();
builder.Services.UseDependencyInjection();
builder.Services.AddSwaggerGen();

var healthCheckBuilder = builder.Services
   .AddHealthChecks();

builder.Services.ConfigureOptions<DatabaseOptionsSetup>();
builder.Services.ConnectDb(healthCheckBuilder, isDevelopment);

builder.Services.ConfigureOptions<RedisOptionsSetup>();
builder.Services.ConfigureRedis(healthCheckBuilder);


builder.Services.RegisterHttpClient(builder.Configuration.GetValue<string>(YandexAPI.YandexGeoAPIKeyConfigName));


var app = builder.Build();

app.UseSerilogRequestLogging(opt =>
{
});
app.UseHealthChecks("/healtz");
app.AddSwagger(isDevelopment);
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
    Log.Information("Service is healthy ;)");
}

app.UseHttpsRedirection();
app.Run();
