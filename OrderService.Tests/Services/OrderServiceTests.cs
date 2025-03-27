using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMarket.OrderService.Exceptions;
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

        private List<Checkpoint> regularCheckpoints = [];
        private List<Checkpoint> deliveryCheckpoints = [];
        private int deliveryCheckpointId => deliveryCheckpoints[0].CheckpointId;
        private int regualarCheckpointId => regularCheckpoints[0].CheckpointId;
        private async Task Seed()
        {
            regularCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(1, 1), false));
            regularCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(2, 2), false));
            deliveryCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(25, 25), true));

            regularCheckpoints.Add(await _checkpointRepository.RegisterPoint(2, new NetTopologySuite.Geometries.Point(12, -2), false));
            deliveryCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(22, 62), true));
        }
        [Fact]
        public async Task Creates_Order()
        {
            var order = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);
            Assert.True(order.Length > 0);
        }
        [Fact]
        public async Task Return_Full_Info()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);
            var info = await _orderService.GetOrderFullInfo(trackNumber);

            Assert.NotNull(info);
            Assert.Equal(trackNumber, info.TrackNumber);
            Assert.NotNull(info.DeliveryPoint);
            Assert.NotNull(info.Checkpoint);
        }

        [Fact]
        public async Task Finds_Closest_DeliveryPoint_If_Same_Supplier_And_DeliveryPoint_Owner()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);
            var info = await _orderService.GetOrderFullInfo(trackNumber);

            Assert.Equal(info.CheckpointId, regularCheckpoints[1].CheckpointId);
        }

        [Fact]
        public async Task Throws_Exception_When_Wrong_DeliveryId()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _orderService.CreateOrder(1, 1, 142142, 1));
        }

        [Fact]
        public async Task Throws_Exception_When_Wrong_Supplier()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _orderService.CreateOrder(1, 1, deliveryCheckpointId, 4122343));
        }
        [Fact]
        public async Task Permits_Selectig_DeliveryPoint_And_Supplypoint_When_Same()
        {
            await DisposeAsync();
            deliveryCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(25, 25), true));
            await Assert.ThrowsAsync<NotFoundException>(() => _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1));
        }


        public async Task DisposeAsync()
        {
            await _respawnDb();
            await _respawnRedis();
            regularCheckpoints = [];
            deliveryCheckpoints = [];
        }

        public Task InitializeAsync()
        {
            return Seed();
        }
    }
}
