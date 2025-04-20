using Bogus;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Services;

namespace WebMarket.OrderService.SupportTools
{
    public class DBSeedService
    {
        private readonly OrdersDbContext _context;
        private readonly ITrackNumberService _numberService;
        private readonly ILogger _logger;
        public DBSeedService(OrdersDbContext context, ITrackNumberService numberService, ILogger<DBSeedService> logger)
        {
            _context = context;
            _numberService = numberService;
            _logger = logger;
        }
        public async Task SeedDb(int checkpointsCnt, int ordersCnt, int seed)
        {
            Randomizer.Seed = new Random(seed);
            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = _context.Database.BeginTransaction();
                    var checkpoints = GetRandomCheckpoints(checkpointsCnt);
                    _context.Checkpoints.AddRange(checkpoints);
                    await _context.SaveChangesAsync();
                    var delivery = await _context.Checkpoints
                        .AsNoTracking()
                        .Where(c => c.IsDeliveryPoint)
                        .Select(c => c.CheckpointId)
                        .ToListAsync();
                    var orders = GetRandomOrders(delivery, ordersCnt);
                    _context.CustomerOrders.AddRange(orders);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                });
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed db");
                throw;
            }
        }
        public List<Checkpoint> GetRandomCheckpoints(int cnt)
        {
            if (cnt == 0)
                return [];
            int owners = cnt / 5;
            var testCheckpoints = new Faker<Checkpoint>();
            testCheckpoints.StrictMode(false)
                .RuleFor(c => c.Address, f => $"{f.Address.StreetAddress()}, {f.Address.BuildingNumber()}")
                .RuleFor(c => c.Location, f => new Point(f.Random.Double(-180, 180), f.Random.Double(-180, 180)))
                .RuleFor(c => c.IsDeliveryPoint, f => f.Random.Bool())
                .RuleFor(c => c.OwnerId, f => f.Random.Int(0, owners + 1));
            
            var res = testCheckpoints.Generate(cnt);
            if(!res.Any(t => t.IsDeliveryPoint))
            {
                res[0].IsDeliveryPoint = true;
            }
            return res;
        }
        public List<CustomerOrder> GetRandomOrders(IEnumerable<int> deliveryPointsIds, int cnt)
        {
            if (cnt == 0)
                return [];
            var testOrders = new Faker<CustomerOrder>();
            testOrders
                .StrictMode(false)
                .RuleFor(o => o.Status, f => OrderStatus.Processing)
                .RuleFor(o => o.DeliveryPointId, f => f.PickRandom(deliveryPointsIds))
                .RuleFor(o => o.CustomerId, f => f.Random.Int(1, cnt))
                .RuleFor(o => o.ProductId, f => f.Random.Int(1, cnt))
                .RuleFor(o => o.TrackNumber, f => _numberService.GetTrackNumber());
                ;
            return testOrders.Generate(cnt);
        }
    }
}
