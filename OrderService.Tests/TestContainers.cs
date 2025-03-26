using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMarket.OrderService.Repositories;

namespace OrderService.Tests
{
    public class TestContainers : BaseIntegrationTest, IAsyncLifetime
    {
        private readonly ICheckpointRepository _checkpointRepository;
        public TestContainers(IntegrationTestWebAppFactory factory) : base(factory)
        {
            _checkpointRepository = _scope.ServiceProvider.GetRequiredService<ICheckpointRepository>();
        }
        [Fact]
        public async Task Redis_Saves_Data()
        {
            await _redis.StringSetAsync("key", "val");
            var val = await _redis.StringGetAsync("key");
            Assert.Equal("val", val.ToString());
        }
        [Fact]
        public async Task Database_Saves_Data()
        {
            var res = await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(1, 1), true);
            var entityFromDb = await _checkpointRepository.GetById(res.CheckpointId);
            Assert.NotNull(entityFromDb);
        }
        [Fact]
        public void Check_Db_Cleaned_Between_Tests()
        {
            //Database_Saves_Data puts one checkpoint so we check if it exists
            Assert.Empty(_context.Checkpoints);
            Assert.Empty(_context.CustomerOrders);
        }
        [Fact]
        public async Task Check_Redis_Cleaned_Between_Tests()
        {
            //Accoridng to Redis_Saves_Data
            var val = await _redis.StringGetAsync("key");
            Assert.True(val.IsNull);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _respawnDb();
            await _respawnRedis();
        }
    }
}
