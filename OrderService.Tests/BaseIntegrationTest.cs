using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMarket.OrderService.Models;

namespace OrderService.Tests
{
    public class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
    {
        protected readonly IServiceScope _scope;
        protected readonly OrdersDbContext _context;
        protected readonly IConnectionMultiplexer _connectionMultiplexer;
        protected readonly Func<Task> _respawnDb;
        protected readonly Func<Task> _respawnRedis;
        protected readonly IDatabase _redis;
        protected BaseIntegrationTest(IntegrationTestWebAppFactory factory) 
        {
            _scope = factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            _connectionMultiplexer = _scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
            _redis = _connectionMultiplexer.GetDatabase();
            _respawnDb = factory.RespawnDbAsync;
            _respawnRedis =  factory.RespawnRedisAsync;
        }

    }
}
