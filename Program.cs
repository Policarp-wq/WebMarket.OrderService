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

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
//extract into the new extension
builder.Services.AddEndpointsApiExplorer();
builder.Services.UseDependencyInjection();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<DatabaseOptionsSetup>();

builder.Services.ConnectDb();
builder.Services.AddHealthChecks().AddDbContextCheck<OrdersDbContext>();

builder.Services.RegisterHttpClient(builder.Configuration.GetValue<string>(YandexAPI.YandexGeoAPIKeyConfigName));      


var app = builder.Build();

app.UseSerilogRequestLogging(opt =>
{
});
app.UseHealthChecks("/healtz");
app.AddSwagger(app.Environment.IsDevelopment());
app.UseExceptionHandler();
app.AddEndpoints();

app.UseHttpsRedirection();
app.Run();


