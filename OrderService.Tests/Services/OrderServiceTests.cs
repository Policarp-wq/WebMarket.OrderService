using Microsoft.Extensions.DependencyInjection;
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

        private List<Checkpoint> deliveryCheckpoints = [];
        private int deliveryCheckpointId => deliveryCheckpoints[0].CheckpointId;
        private async Task Seed()
        {
            deliveryCheckpoints.Add(await _checkpointRepository.RegisterPoint(1, new NetTopologySuite.Geometries.Point(25, 25), true));

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
        }

        [Fact]
        public async Task Finds_Closest_DeliveryPoint_If_Same_Supplier_And_DeliveryPoint_Owner()
        {

        }

        [Fact]
        public async Task Throws_Exception_When_Wrong_DeliveryId()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrder(1, 1, 142142, 1));
        }

        [Fact]
        public async Task Throws_Exception_When_Wrong_Supplier()
        {
            //await Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrder(1, 1, deliveryCheckpointId, 4122343));
        }
        [Fact]
        public async Task Returns_Full_Tracking_Info()
        {
        }

        [Fact]
        public async Task Ignores_Same_Update_Data()
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
