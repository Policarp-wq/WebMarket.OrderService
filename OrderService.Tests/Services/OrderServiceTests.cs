using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.Services;

namespace OrderService.Tests.Services
{
    public class OrderServiceTests : BaseIntegrationTest, IAsyncLifetime
    {
        private readonly IOrderService _orderService;
        private readonly ICheckpointRepository _checkpointRepository;
        public OrderServiceTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
            _orderService = _scope.ServiceProvider.GetRequiredService<IOrderService>();
            _checkpointRepository = _scope.ServiceProvider.GetRequiredService<ICheckpointRepository>();
        }

        private List<Checkpoint> seededCheckpoints = [];
        private List<Checkpoint> deliveryCheckpoints = [];
        private int deliveryCheckpointId => deliveryCheckpoints[0].CheckpointId;
        private int regualrCheckpointId => seededCheckpoints[0].CheckpointId;
        private async Task Seed()
        {
            seededCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(1, 1), false));
            seededCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(2, 2), false));
            deliveryCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(25, 25), true));

            seededCheckpoints.Add(await _checkpointRepository.RegisterPoint(2, new NetTopologySuite.Geometries.Point(12, -2), false));
            deliveryCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(22, 62), true));
        }
        //mock for yandex api
        [Fact]
        public async Task Creates_Order()
        {
            var order = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);
            Assert.NotNull(order);
        }

        public async Task DisposeAsync()
        {
            await _respawnDb();
            await _respawnRedis();
            seededCheckpoints = [];
            deliveryCheckpoints = [];
        }

        public Task InitializeAsync()
        {
            return Seed();
        }
    }
}
