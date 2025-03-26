using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.SupportTools.TrackNumber;
using WebMarket.OrderService.ApiContracts;
using System.Diagnostics;
using Xunit.Abstractions;

namespace OrderService.Tests.Repositories
{

    public class OrderRepositoryTests : BaseIntegrationTest, IAsyncLifetime
    {
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ITrackNumberGenerator _tracknumberGenerator;
        public OrderRepositoryTests(IntegrationTestWebAppFactory factory) :base(factory)
        {
            _checkpointRepository = _scope.ServiceProvider.GetRequiredService<ICheckpointRepository>();
            _orderRepository = _scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            _tracknumberGenerator = _scope.ServiceProvider.GetRequiredService<ITrackNumberGenerator>();
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
        [Fact] 
        public async Task Returns_Order_With_Checkpoint_And_DeliveryPoint()
        {
            var order = await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "000000000");
            Assert.NotNull(order);
            Assert.NotNull(order.Checkpoint);
            Assert.NotNull(order.DeliveryPoint);
            Assert.Equal(CustomerOrder.OrderStatus.Processing, order.Status);
        }

        [Fact]
        public async Task Throws_Exception_When_Proivided_Not_Delivry_Point()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _orderRepository.CreateOrder(1, 1, regualrCheckpointId, seededCheckpoints[1].CheckpointId, "000000000"));
            
        }

        [Fact] // indexes not resetting
        public async Task Created_Order_Has_Processing_Status()
        {
            var order = await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "000000000");
            Assert.Equal(CustomerOrder.OrderStatus.Processing, order.Status);
        }

        [Fact]
        public async Task Throws_Exception_When_TrackNumber_NotValid()
        {
            await Assert.ThrowsAsync<DbUpdateException>(() => _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "1213"));
        }

        [Fact]
        public async Task Throws_Exception_When_Trying_Add_Same_DeliveryPoint_And_Supply()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _orderRepository.CreateOrder(2, 4, deliveryCheckpointId, deliveryCheckpointId, "000000000"));
            await Assert.ThrowsAsync<ArgumentException>(() => _orderRepository.CreateOrder(2, 4, regualrCheckpointId, regualrCheckpointId, "000000000"));
        }

        [Fact]
        public async Task Throws_Exception_When_Trying_Add_Same_TrackNumber()
        {
            var order = await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "000000000");
            await Assert.ThrowsAsync<DbUpdateException>(() => _orderRepository.CreateOrder(2, 4, deliveryCheckpointId, regualrCheckpointId, "000000000"));
        }
        [Fact]
        public async Task Finds_Order_By_Id()
        {
            var order = await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "000000000");
            await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "111111111");
            await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "222222222");

            var fromDb = await _orderRepository.GetOrderInfo(order.OrderId);

            Assert.NotNull(fromDb);
            Assert.NotNull(fromDb.Checkpoint);
            Assert.NotNull(fromDb.DeliveryPoint);

            Assert.Equal(fromDb.TrackNumber, order.TrackNumber);
        }

        [Fact]
        public async Task Finds_Order_By_TrackNumber()
        {
            var order = await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "000000000");
            await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "111111111");
            await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "222222222");

            var fromDb = await _orderRepository.GetOrderInfo("000000000");

            Assert.NotNull(fromDb);
            Assert.NotNull(fromDb.Checkpoint);
            Assert.NotNull(fromDb.DeliveryPoint);

            Assert.Equal(fromDb.OrderId, order.OrderId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Saves_All(int ordersCnt)
        {
            for (int i = 0; i < ordersCnt; i++)
            {
                await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, _tracknumberGenerator.GenerateTrackNumber());
            }
            var orders = await _orderRepository.ListOrders();

            Assert.Equal(ordersCnt, orders.Count);
        }
        [Theory]
        [InlineData(CustomerOrder.OrderStatus.Packing_up)]
        [InlineData(CustomerOrder.OrderStatus.Delivering)]
        [InlineData(CustomerOrder.OrderStatus.Delivered)]
        [InlineData(CustomerOrder.OrderStatus.Completed)]
        [InlineData(CustomerOrder.OrderStatus.Denied)]
        public async Task Updates_Status(CustomerOrder.OrderStatus status)
        {
            var order = await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, regualrCheckpointId, "000000000");
            var report = await _orderRepository.UpdateOrderInfo(new OrderUpdateInfo("000000000", null, status));
            
            Assert.NotNull(order);
            Assert.NotNull(report);
            Assert.NotNull(report.OrderInfo.Checkpoint);
            Assert.NotNull(report.OrderInfo.DeliveryPoint);
            Assert.True(report.Changed);
            Assert.Equal(order.TrackNumber, report.OrderInfo.TrackNumber);
            Assert.Equal(status, report.OrderInfo.Status);

            order = await _orderRepository.GetOrderInfo("000000000");
            
            Assert.NotNull(order);
            Assert.Equal(status, order.Status);
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
