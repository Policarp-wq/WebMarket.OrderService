using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebMarket.OrderService.DTO.OrderStatusStory;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public class OrderStatusStoryRepository : BaseRepository<OrderStatusStory>, IOrderStatusStoryRepository
    {
        private readonly IOrderRepository _orderRepository;
        public OrderStatusStoryRepository(OrdersDbContext context, IOrderRepository orderRepository) : base(context, context => context.OrderStatuses)
        {
            _orderRepository = orderRepository;
        }

        private async Task<OrderStatusStory?> GetLatestTrackableStory(int orderId)
        {
            return await _dbSet
                 .Where(t => t.OrderId == orderId)
                 .OrderBy(t => t.ChangeDate)
                 .LastOrDefaultAsync();
        }

        private async Task<OrderStatusUnit?> GetLatestUnit(int orderId)
        {
            return await _dbSet
                .AsNoTracking()
                 .Where(t => t.OrderId == orderId)
                 .Select(t => new OrderStatusUnit(t.Status, t.ChangeDate))
                 .OrderBy(t => t.ChangeDate)
                 .LastOrDefaultAsync();
        }

        public async Task<OrderStatusUnit> GetLatestStatus(int orderId)
        {
            var latest = await GetLatestUnit(orderId);
            if (latest == null)
                throw new NotFoundException($"No status for order {orderId}");
            return latest;
        }

        public async Task<IEnumerable<OrderStatusUnit>> GetOrderStory(int orderId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.OrderId == orderId)
                .Select(s => new OrderStatusUnit(s.Status, s.ChangeDate))
                .OrderBy(s => s.ChangeDate)
                .ToListAsync();
        }

        public async Task<bool> InitOrder(int orderId)
        {
            _dbSet.Add(new OrderStatusStory()
            {
                OrderId = orderId,
                Status = CustomerOrder.InitialStatus,
                ChangeDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatus(int orderId, OrderStatus status)
        {
            var latest = await GetLatestTrackableStory(orderId);
            if (latest == null)
                throw new NotFoundException($"No status for order {orderId}");
            if(latest.Status == status)
                return false;
            if (status != OrderStatus.Denied && latest.Status > status)
                throw new ArgumentException($"Attempt to assing status that comes before latest {latest.Status}: {status}");
            using(var transaction = _context.Database.BeginTransaction())
            {
                _dbSet.Add(new OrderStatusStory()
                {
                    OrderId = orderId,
                    Status = status,
                    ChangeDate = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                await _orderRepository.UpdateOrderInfo(orderId, status);
                return true;
            }
           
            
        }
    }
}
