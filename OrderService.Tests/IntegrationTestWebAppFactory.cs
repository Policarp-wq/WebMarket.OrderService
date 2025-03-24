using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;
using WebMarket.OrderService.Models;

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
        private DbConnection _connection = default!;
        public Respawner Respawner { get; private set; } = default!;    

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services
                    .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<OrdersDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<OrdersDbContext>(options =>
                {
                    options
                        .UseNpgsql(_dbContainer.GetConnectionString());
                });
            });
        }
        public async Task RespawnDbAsync()
        {
            await Respawner.ResetAsync(_connection);
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            //Path looks awful
            var migrationSql = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "../../../", "init.sql"));
            await _dbContainer.ExecScriptAsync(migrationSql);
            _connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
            await _connection.OpenAsync();
            await InitializeRespawner();
        }

        private async Task InitializeRespawner()
        {
            Respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
        }

        public new Task DisposeAsync()
        {
            return _dbContainer.StopAsync();
        }
    }
}
