using WebMarket.OrderService.AppExtensions;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Options;
using WebMarket.OrderService.SupportTools.MapSupport;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.UseDependencyInjection();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<DatabaseOptionsSetup>();
builder.Services.ConnectDb();
builder.Services.RegisterHttpClient(builder.Configuration.GetValue<string>(YandexAPI.YandexGeoAPIKeyConfigName));      

var app = builder.Build();

app.AddSwagger(app.Environment.IsDevelopment());
app.UseExceptionHandler();
app.AddEndpoints();

app.UseHttpsRedirection();

app.Run();


