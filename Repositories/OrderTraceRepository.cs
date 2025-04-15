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

        public async Task<bool> AddRouteUnit(int orderId, int checkpointId)
        {
            var last = await GetLastTrackable(orderId);
            if(last != null)
            {
                if (last.DeliveryStatus == DeliveryStatus.Delivering_to)
                {
                    throw new ArgumentException($"Tried to add new route unit while delivering to the current");
                }
                last.DeliveryStatus = DeliveryStatus.Sent;
            }  
            _dbSet.Add(new OrderTrace
            { 
                OrderId = orderId,
                CheckpointId = checkpointId,
                DeliveryStatus = OrderTrace.InitStatus,
                DeliveryDate = null
            });
            await _context.SaveChangesAsync();
            return true;
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
                    x.DeliveryDate,
                    x.DeliveryStatus
                ))
                .ToListAsync();
        }
        private async Task<OrderTrace?> GetLastTrackable(int orderId)
        {
            return await _dbSet
                .Where(t => t.OrderId == orderId)
                .OrderBy(t => t.DeliveryDate)
                .LastOrDefaultAsync();
        }
        //disable!
        public async Task<bool> UpdateLastTraceInfo(int orderId, DeliveryStatus status)
        {
            var lastTrace = await GetLastTrackable(orderId);
            if (lastTrace == null)
                throw new ArgumentException($"Provided order id {orderId} has no trace but attempted to update status to {status}");
            if (lastTrace.DeliveryStatus > status)
                throw new ArgumentException($"Attempt to update status that logically is earlier than current {lastTrace.DeliveryStatus}. Tried: {status}");
            if(lastTrace.DeliveryStatus == status)
                return false;
            lastTrace.DeliveryStatus = status;
            await _context.SaveChangesAsync();
            return true;
        }
        //Sets delivery status to sorts cuz setting deliverytime means it has been delivered to checkpoint
        public async Task<bool> SetOrderDeliveredTime(int orderId, DateTime deliveredTime)
        {
            var lastTrace = await GetLastTrackable(orderId);
            if (lastTrace == null)
                throw new ArgumentException($"Provided order id {orderId} has no trace but attempted to update delivery time to {deliveredTime}");
            if (lastTrace.DeliveryStatus != DeliveryStatus.Delivering_to)
                throw new ArgumentException($"Attempted to update delivery time for the order that is not delivering");
            lastTrace.DeliveryStatus = DeliveryStatus.Sorting;
            lastTrace.DeliveryDate = deliveredTime;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
