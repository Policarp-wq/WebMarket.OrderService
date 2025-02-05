using Microsoft.EntityFrameworkCore;
using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public class OrderRepository : BaseRepository<CustomerOrder>, IOrderRepository
    {
        public OrderRepository(OrdersDbContext context) : base(context, context => context.CustomerOrders)
        {
        }

        public async Task<int> CreateOrder(int customerID, int productId, int deliveryPointID)
        {
            var res = await _dbSet.AddAsync(new CustomerOrder()
            {
                CustomerId = customerID,
                ProductId = productId,
                CheckpointId = deliveryPointID,
                DeliveryPointId = deliveryPointID,
            });
            await _context.SaveChangesAsync();
            return res.Entity.OrderId;
        }

        public async Task<CustomerOrder?> GetOrderInfo(int trackNumber)
        {
            if(trackNumber < 0) throw new InvalidArgumentException("track number < 0: " +  trackNumber);
            var res = await _dbSet
                .Include(o => o.Checkpoint)
                .Include(o => o.DeliveryPoint)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == trackNumber);
            return res;
            
                
                
        }

        public Task<bool> UpdateOrderInfo(int CheckpointId, CustomerOrder.OrderStatus status)
        {
            throw new NotImplementedException();
        }

        Task<OrderInfo> IOrderRepository.GetOrderInfo(int trackNumber)
        {
            throw new NotImplementedException();
        }
    }
}
