using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using Testcontainers.PostgreSql;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;

namespace OrderService.Tests
{
    public class CheckpointTests : BaseIntegrationTest
    {
        private static Random _random = new Random();
        private static double GetRandomCoordinate() => _random.NextDouble() * 360 - 180;
        private static Point GetRandomPoint() => new Point(GetRandomCoordinate(), GetRandomCoordinate());
        private readonly ICheckpointRepository _checkpointRepository;
        public CheckpointTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
            _checkpointRepository = _scope.ServiceProvider.GetRequiredService<ICheckpointRepository>();
        }
        private static Checkpoint CreateCheckpoint(int id, int ownerId, bool isDelivery)
        {
            return new Checkpoint()
            {
                CheckpointId = id,
                IsDelivryPoint = isDelivery,
                OwnerId = ownerId,
                Location = GetRandomPoint()
            };
        }

        [Fact]
        public async Task Test_DbSet_Reachability()
        {
            var points = await _checkpointRepository.GetAll();
            Assert.Empty(points);
        }

    
    }
}
