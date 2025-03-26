using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using StackExchange.Redis;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;

namespace OrderService.Tests.Repositories
{
    public class CheckpointRepositoryTests : BaseIntegrationTest, IAsyncLifetime
    {
        private static Random _random = new Random();
        private static double GetRandomCoordinate() => _random.NextDouble() * 360 - 180;
        private static Point GetRandomPoint() => new Point(GetRandomCoordinate(), GetRandomCoordinate());
        private readonly ICheckpointRepository _checkpointRepository;
        private Func<Task> _respawnDb;
        public CheckpointRepositoryTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
            _checkpointRepository = _scope.ServiceProvider.GetRequiredService<ICheckpointRepository>();
            _respawnDb = () => factory.RespawnDbAsync();
        }
        private static Checkpoint CreateCheckpoint(int id, int ownerId, bool isDelivery)
        {
            return new Checkpoint()
            {
                CheckpointId = id,
                IsDeliveryPoint = isDelivery,
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
        [Fact]
        public async Task Test_Checkpoints_Add_Correctly()
        {
            await _checkpointRepository.RegisterPoint(1, GetRandomPoint(), false);
            await _checkpointRepository.RegisterPoint(2, GetRandomPoint(), false);
            await _checkpointRepository.RegisterPoint(3, GetRandomPoint(), false);
            var points = await _checkpointRepository.GetAll();

            Assert.Equal(3, points.Count);
        }

        [Fact]
        public async Task Test_Checkpoints_With_Same_Owner_Returns_All()
        {
            await _checkpointRepository.RegisterPoint(1, GetRandomPoint(), false);
            await _checkpointRepository.RegisterPoint(1, GetRandomPoint(), true);
            await _checkpointRepository.RegisterPoint(1, GetRandomPoint(), false);

            var points = await _checkpointRepository.GetCheckpointsIdByOwner(1);

            Assert.Equal(3, points.Count);
        }

        [Fact]
        public async Task Test_Checkpoints_With_Wrong_Owner_Empty()
        {
            await _checkpointRepository.RegisterPoint(1, GetRandomPoint(), false);
            await _checkpointRepository.RegisterPoint(1, GetRandomPoint(), true);
            await _checkpointRepository.RegisterPoint(1, GetRandomPoint(), false);

            var points = await _checkpointRepository.GetCheckpointsIdByOwner(2);
            Assert.Empty(points);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Test_Return_Only_Delivery_Checkpoints(int pointsCnt)
        {
            int delivery = 0;
            for(int i = 0; i < pointsCnt; i++)
            {
                bool isDelivery = _random.Next(2) == 1 ? true : false;
                if(isDelivery)
                    delivery++;
                await _checkpointRepository.RegisterPoint(1, GetRandomPoint(), isDelivery);
            }
            var points = await _checkpointRepository.GetDeliveryPoints();
            Assert.Equal(delivery, points.Count);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync() => _respawnDb();
    }
}
