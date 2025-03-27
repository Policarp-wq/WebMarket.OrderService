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
using WebMarket.OrderService.ApiContracts;

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
        private int regularCheckpointId => regularCheckpoints[0].CheckpointId;
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
        public async Task Returns_Full_Info()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);
            var info = await _orderService.GerOrderInfo(trackNumber);

            Assert.NotNull(info);
            Assert.Equal(trackNumber, info.TrackNumber);
            Assert.NotNull(info.DeliveryPoint);
            Assert.NotNull(info.Checkpoint);
        }

        [Fact]
        public async Task Finds_Closest_DeliveryPoint_If_Same_Supplier_And_DeliveryPoint_Owner()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);
            var info = await _orderService.GerOrderInfo(trackNumber);

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
        [Fact]
        public async Task Returns_Full_Tracking_Info()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);

            var trackInfo = await _orderService.GetTrackingInfo(trackNumber);
            Assert.NotNull(trackInfo);
            Assert.Equal(trackNumber, trackInfo.TrackNumber);
            Assert.True(trackInfo.CurrentAddress.Length > 0);
            Assert.True(trackInfo.DeliveryAddress.Length > 0);
        }
        [Theory]
        [InlineData(CustomerOrder.OrderStatus.Packing_up)]
        [InlineData(CustomerOrder.OrderStatus.Delivering)]
        [InlineData(CustomerOrder.OrderStatus.Delivered)]
        [InlineData(CustomerOrder.OrderStatus.Completed)]
        [InlineData(CustomerOrder.OrderStatus.Denied)]
        public async Task Updates_Order_When_Changed_Checkpoint_And_Status(CustomerOrder.OrderStatus status)
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);

            var report = await _orderService.UpdateOrder(new OrderUpdateInfo(trackNumber, regularCheckpoints[1].CheckpointId, status));
            Assert.True(report);

            var updated = await _orderService.GetOrderInfo(trackNumber);
            Assert.Equal(status, updated.Status);
            Assert.Equal(regularCheckpoints[1].CheckpointId, updated.Checkpoint.CheckpointId);

        }
        [Fact]
        public async Task Not_Change_DeliveryPoint_On_Update()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);

            var report = await _orderService.UpdateOrder(new OrderUpdateInfo(trackNumber, regularCheckpoints[1].CheckpointId, CustomerOrder.OrderStatus.Packing_up));
            Assert.True(report);

            var updated = await _orderService.GetOrderInfo(trackNumber);
            Assert.Equal(deliveryCheckpointId, updated.DeliveryPoint.CheckpointId);
        }
        [Fact]
        public async Task Ignores_Same_Update_Data()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);

            var report = await _orderService.UpdateOrder(new OrderUpdateInfo(trackNumber, regularCheckpoints[1].CheckpointId, CustomerOrder.OrderStatus.Packing_up));
            Assert.True(report);
            report = await _orderService.UpdateOrder(new OrderUpdateInfo(trackNumber, regularCheckpoints[1].CheckpointId, CustomerOrder.OrderStatus.Packing_up));
            Assert.False(report);
        }
        [Fact]
        public async Task Ignores_Null_Update_Data()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);

            var report = await _orderService.UpdateOrder(new OrderUpdateInfo(trackNumber, null, null));
            Assert.False(report);
            var updated = await _orderService.GetOrderInfo(trackNumber);
            Assert.Equal(deliveryCheckpointId, updated.DeliveryPoint.CheckpointId);
            Assert.Equal(CustomerOrder.DefaultStatus, updated.Status);
        }
        [Fact]
        public async Task Updates_Particial_Data_Status()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);

            var report = await _orderService.UpdateOrder(new OrderUpdateInfo(trackNumber, null, CustomerOrder.OrderStatus.Packing_up));
            Assert.True(report);
            var updated = await _orderService.GetOrderInfo(trackNumber);

            //Assert.Equal(deliveryCheckpointId, updated.Checkpoint.CheckpointId);
            Assert.Equal(CustomerOrder.OrderStatus.Packing_up, updated.Status);
        }
        [Fact]
        public async Task Updates_Particial_Data_Checkpoint()
        {
            var trackNumber = await _orderService.CreateOrder(1, 1, deliveryCheckpointId, 1);

            var report = await _orderService.UpdateOrder(new OrderUpdateInfo(trackNumber, regularCheckpoints[2].CheckpointId, null));
            Assert.True(report);
            var updated = await _orderService.GetOrderInfo(trackNumber);

            Assert.Equal(regularCheckpoints[2].CheckpointId, updated.Checkpoint.CheckpointId);
            Assert.Equal(CustomerOrder.DefaultStatus, updated.Status);
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
