using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data.Common;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Options;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.Services;
using WebMarket.OrderService.SupportTools;
using WebMarket.OrderService.SupportTools.MapSupport;

namespace WebMarket.OrderService.AppExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConnectDb(this IServiceCollection services)
        {
            
            services.AddDbContext<OrdersDbContext>((serviceProvider, dbContextOptBuilder) =>
            {
                var dbOpt = serviceProvider.GetService<IOptions<DatabaseOptions>>()!.Value;
                dbContextOptBuilder.UseNpgsql(dbOpt.ConnectionString, sqlOpt =>
                {
                    sqlOpt.UseNetTopologySuite();
                    sqlOpt.CommandTimeout(dbOpt.CommandTimeout);
                    sqlOpt.EnableRetryOnFailure(dbOpt.CommandTimeout);
                });
                dbContextOptBuilder.EnableDetailedErrors(dbOpt.EnabledDetailedErrors);
                dbContextOptBuilder.EnableSensitiveDataLogging(dbOpt.EnabledSensitiveDataLog);
                dbContextOptBuilder.LogTo(Console.WriteLine);
            });

            return services;
        }

        public static IServiceCollection UseDependencyInjection(this IServiceCollection services)
        {
            services.AddScoped<ICheckpointService, CheckpointService>();
            services.AddScoped<ICheckpointRepository, CheckpointRepository>();
            return services;
        }

        public static IServiceCollection RegisterHttpClient(this IServiceCollection services, string yandexApiKey)
        {
            services.AddHttpClient();
            services.AddSingleton<IMapGeocoder, YandexAPI>(x =>
                new YandexAPI(x.GetRequiredService<IHttpClientFactory>(), yandexApiKey));
            return services;
        }
    }
}
