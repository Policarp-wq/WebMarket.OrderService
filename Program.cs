using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Text.Json.Serialization;
using WebMarket.OrderService.AppExtensions;
using WebMarket.OrderService.Auth;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Options;
using WebMarket.OrderService.SupportTools;
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
    //не конвертирует
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
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

builder.Services.AddJWTAuth(builder.Configuration.GetSection(JwtOptionsSetup.JWT_SECTION).Get<JwtOptions>()!);

 
builder.Services.RegisterHttpClient(builder.Configuration.GetValue<string>(YandexAPI.YandexGeoAPIKeyConfigName)!);
if (isDevelopment)
{
    builder.Services.AddSeeder();
}

var app = builder.Build();

app.UseSerilogRequestLogging(opt =>
{
});
app.UseHealthChecks("/healtz");
app.UseSwagger(isDevelopment);
app.UseExceptionHandler();

var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();
var report = await healthCheckService.CheckHealthAsync();
if (report.Status != HealthStatus.Healthy)
{
    Log.Fatal("Health check failed at startup: {Status}", report.Status);
    foreach (var entry in report.Entries)
    {
        Log.Fatal("{JWT_SECTION}: {Value}", entry.Key, entry.Value.Status);
    }
    return;
}
else
{
    Log.Information("Service is healthy ;)");
}
if (false && isDevelopment)
{
    Console.WriteLine("SEEDING MODE. CONFIRM ACTION BY TYPING 'YES'");
    using IServiceScope scope = app.Services.CreateScope();
    var seed = scope.ServiceProvider.GetRequiredService<DBSeedService>();
    if (seed != null && "yes".Equals(Console.ReadLine()?.ToLower()))
    {
        Console.WriteLine("CONFIRMED");
        await seed.SeedDb(15, 40, 1337420);
        Console.WriteLine("SEEDED");
    }
}
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints();
app.Run();

public partial class Program { }
