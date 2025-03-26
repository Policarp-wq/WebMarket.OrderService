using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NetTopologySuite.Geometries;
using Npgsql;
using Respawn;
using StackExchange.Redis;
using System.Data.Common;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using WebMarket.OrderService.AppExtensions;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.SupportTools.Kafka;
using WebMarket.OrderService.SupportTools.MapSupport;

namespace OrderService.Tests
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:17-3.5")
            .WithDatabase("orders_managment")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        private readonly RedisContainer _redisContainer = new RedisBuilder()
            .WithImage("redis:7.4")
            .Build();
        private DbConnection _connection = default!;
        public Respawner Respawner { get; private set; } = default!;    

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                ReplaceDb(services);
                ReplaceRedis(services);
                ReplaceMapGeoCoder(services);
                ReplaceKafkaProducer(services);
            });
        }

        private void RemoveService<T>(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(T));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }
        }

        private void ReplaceKafkaProducer(IServiceCollection services)
        {
            var mock = new Mock<IKafkaMessageProducer>();
            mock.Setup(k => k.ProduceMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()).Result)
                .Returns(new Confluent.Kafka.DeliveryResult<string, string>());
            services.AddSingleton<IKafkaMessageProducer>(mock.Object);
        }

        private void ReplaceMapGeoCoder(IServiceCollection services)
        {
            RemoveService<IMapGeocoder>(services);
            var mock = new Mock<IMapGeocoder>(MockBehavior.Strict);
            mock.Setup(geo => geo.GetLongLatByAddress(It.IsAny<string>()).Result).Returns("Some addr");
            mock.Setup(geo => geo.GetAddressByLongLat(It.IsInRange<double>(-180, 180, Moq.Range.Inclusive),
                It.IsInRange<double>(-180, 180, Moq.Range.Inclusive)).Result).Returns("Some addr");
            mock.Setup(geo => geo.GetAddressByLongLat(It.Is<double>(c => Math.Abs(c) > 180),
                It.Is<double>(c => Math.Abs(c) > 180)).Result).Throws<ArgumentException>();
            mock.Setup(geo => geo.GetAddressByLongLat(It.Is<Point>(p => p.IsGeo())).Result).Returns("Some addr");
            mock.Setup(geo => geo.GetAddressByLongLat(It.Is<Point>(p => !p.IsGeo())).Result).Throws<ArgumentException>();

            services.AddSingleton<IMapGeocoder>(mock.Object);
        }

        private void ReplaceDb(IServiceCollection services)
        {
            RemoveService<DbContextOptions<OrdersDbContext>>(services);
            services.AddDbContext<OrdersDbContext>(options =>
            {
                options
                    .UseNpgsql(_dbContainer.GetConnectionString() + ";Include Error Detail=true;")
                    .EnableSensitiveDataLogging()
                    ;
            });
        }

        private void ReplaceRedis(IServiceCollection services)
        {
            RemoveService<IConnectionMultiplexer>(services);
            services.AddSingleton<IConnectionMultiplexer>(x =>
            {
                return ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
            });
        }

        public async Task RespawnDbAsync()
        {
             await Respawner.ResetAsync(_connection);
        }

        public async Task RespawnRedisAsync()
        {
            await _redisContainer.ExecScriptAsync("FLUSHALL SYNC");
        }  

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            //Path looks awful
            var migrationSql = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "../../../", "init.sql"));
            await _dbContainer.ExecScriptAsync(migrationSql);
            _connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
            await _connection.OpenAsync();
            Respawner = await CreateRespawner(_connection);

            await _redisContainer.StartAsync();
        }

        public Task<Respawner> CreateRespawner(DbConnection connection)
        {
            return Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
            });
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _redisContainer.StopAsync();
        }
    }
}
