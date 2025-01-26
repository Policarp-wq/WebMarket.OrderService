using WebMarket.OrderService.AppExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.AddSwagger(app.Environment.IsDevelopment());

app.AddEndpoints();

app.UseHttpsRedirection();

app.Run();


