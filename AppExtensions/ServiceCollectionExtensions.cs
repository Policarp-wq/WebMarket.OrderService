using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using StackExchange.Redis;
using System.Data.Common;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Options;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.Services;
using WebMarket.OrderService.SupportTools;
using WebMarket.OrderService.SupportTools.Kafka;
using WebMarket.OrderService.SupportTools.MapSupport;
using WebMarket.OrderService.SupportTools.TrackNumber;

namespace WebMarket.OrderService.AppExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConnectDb(this IServiceCollection services, IHealthChecksBuilder healthChecksBuilder)
        {
            
            services.AddDbContext<OrdersDbContext>((serviceProvider, dbContextOptBuilder) =>
            {
                var dbOpt = serviceProvider.GetService<IOptions<DatabaseOptions>>()!.Value;
                dbContextOptBuilder.UseNpgsql(dbOpt.ConnectionString, sqlOpt =>
                {
                    sqlOpt.MapEnum<CustomerOrder.OrderStatus>("order_status");
                    sqlOpt.UseNetTopologySuite();
                    sqlOpt.CommandTimeout(dbOpt.CommandTimeout);
                    sqlOpt.EnableRetryOnFailure(dbOpt.CommandTimeout);
                });
                dbContextOptBuilder.EnableDetailedErrors(dbOpt.EnabledDetailedErrors);
                dbContextOptBuilder.EnableSensitiveDataLogging(dbOpt.EnabledSensitiveDataLog);
                dbContextOptBuilder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
            });
            healthChecksBuilder.AddDbContextCheck<OrdersDbContext>();

            return services;
        }

        public static IServiceCollection ConfigureRedis(this IServiceCollection services, IHealthChecksBuilder healthChecksBuilder)
        {
            services.AddSingleton<IConnectionMultiplexer>(x =>
            {
                var options = x.GetService<IOptions<RedisOptions>>()!.Value;
                var configuration = ConfigurationOptions.Parse(options.ConnectionString);
                configuration.ConnectRetry = options.ConnectRetry;
                configuration.ConnectTimeout = options.ConnectTimeout;
                configuration.ClientName = "Redis:" + Environment.MachineName;
                
                return ConnectionMultiplexer.Connect(configuration);
            });

            healthChecksBuilder.AddRedis(x =>
            {
                var redisOpt = x.GetService<IOptions<RedisOptions>>()!.Value;
                return redisOpt.ConnectionString;
            });
            
            return services;
        }

        public static IServiceCollection UseDependencyInjection(this IServiceCollection services)
        {
            services.AddScoped<ICheckpointService, CheckpointService>();
            services.AddScoped<ICheckpointRepository, CheckpointRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, WebMarket.OrderService.Services.OrderService>();
            services.AddSingleton<ITrackNumberGenerator, TrackNumberGenerator>();
            return services;
        }

        public static IServiceCollection RegisterKafkaProducer(this IServiceCollection services, IConfigurationSection producerSection)
        {
            var producerConf = new ProducerConfig();
            producerSection.Bind(producerConf);
            services.AddSingleton<IKafkaMessageProducer>(x =>
            {
                return new KafkaMessageProducer(x.GetRequiredService<ILogger<KafkaMessageProducer>>(),
                    new ProducerBuilder<string, string>(producerConf).Build());
            });
            return services;
        }
        public static IServiceCollection RegisterHttpClient(this IServiceCollection services, string yandexApiKey)
        {
            services.AddHttpClient();
            services.AddSingleton<IMapGeocoder, YandexAPI>(x =>
                new YandexAPI(x.GetRequiredService<IHttpClientFactory>(), x.GetRequiredService<IConnectionMultiplexer>(), x.GetRequiredService<ILogger<YandexAPI>>(), yandexApiKey));
            return services;
        }
    }
}
