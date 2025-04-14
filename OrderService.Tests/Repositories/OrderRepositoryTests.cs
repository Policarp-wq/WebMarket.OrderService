using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.SupportTools.TrackNumber;
using WebMarket.OrderService.DTO;

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
        private List<Checkpoint> deliveryCheckpoints = [];
        private int deliveryCheckpointId => deliveryCheckpoints[0].CheckpointId;
        private async Task Seed()
        {
            deliveryCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(25, 25), true));
            deliveryCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(22, 62), true));
        }
        [Fact] 
        public async Task Returns_Order_With_Checkpoint_And_DeliveryPoint()
        {
            
        }

        [Fact]
        public async Task Throws_Exception_When_Proivided_Not_Delivry_Point()
        {
           
            
        }

        [Fact] // indexes not resetting
        public async Task Created_Order_Has_Processing_Status()
        {
           
        }

        [Fact]
        public async Task Throws_Exception_When_TrackNumber_NotValid()
        {
            await Assert.ThrowsAsync<DbUpdateException>(() => _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, "1213"));
        }


        [Fact]
        public async Task Throws_Exception_When_Trying_Add_Same_TrackNumber()
        {
            var order = await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, "000000000");
            await Assert.ThrowsAsync<DbUpdateException>(() => _orderRepository.CreateOrder(2, 4, deliveryCheckpointId, "000000000"));
        }
        [Fact]
        public async Task Finds_Order_By_Id()
        {
            var order = await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, "000000000");
            await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, "111111111");
            await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, "222222222");

            var fromDb = await _orderRepository.GetOrderInfo(order.OrderId);

            Assert.NotNull(fromDb);
            Assert.NotNull(fromDb.DeliveryPoint);

            Assert.Equal(fromDb.TrackNumber, order.TrackNumber);
        }

        [Fact]
        public async Task Finds_Order_By_TrackNumber()
        {
            var order = await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, "000000000");
            await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, "111111111");
            await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, "222222222");

            var fromDb = await _orderRepository.GetOrderInfo("000000000");

            Assert.NotNull(fromDb);
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
                await _orderRepository.CreateOrder(1, 1, deliveryCheckpointId, _tracknumberGenerator.GenerateTrackNumber());
            }
            var orders = await _orderRepository.ListOrders();

            Assert.Equal(ordersCnt, orders.Count);
        }
        public async Task Updates_Status()
        {
            
        }

        public async Task DisposeAsync()
        {
            await _respawnDb();
            await _respawnRedis();
            deliveryCheckpoints = [];
        }

        public Task InitializeAsync()
        {
            return Seed();
        }
    }
}
