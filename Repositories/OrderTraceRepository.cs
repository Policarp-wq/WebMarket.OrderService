using Microsoft.EntityFrameworkCore;
using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public class OrderTraceRepository : BaseRepository<OrderTrace>, IOrderTraceRepository
    {
        public OrderTraceRepository(OrdersDbContext context) : base(context, context => context.OrderTraceses)
        {
        }
        

        public async Task<IEnumerable<OrderTraceRouteUnit>> GetOrderRoute(int OrderId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Checkpoint)
                .Where(x => x.OrderId == OrderId)
                .OrderBy(x => x.DeliveryDate)
                .Select(x =>
                new OrderTraceRouteUnit
                (
                    x.Checkpoint.Address,
                    x.DeliveryDate
                ))
                .ToListAsync();
        }
    }
}
