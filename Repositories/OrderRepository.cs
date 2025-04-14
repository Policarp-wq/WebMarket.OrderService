using Microsoft.EntityFrameworkCore;
using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public class OrderRepository : BaseRepository<CustomerOrder>, IOrderRepository
    {
        public OrderRepository(OrdersDbContext context) : base(context, context => context.CustomerOrders)
        {
        }

        public async Task<CustomerOrder> CreateOrder(int customerID, int productId, int deliveryPointID, string trackNumber)
        {
            var deliveryPoint = await _context.Checkpoints.FindAsync(deliveryPointID);
            if (deliveryPoint == null || !deliveryPoint.IsDeliveryPoint)
                throw new ArgumentException($"Provided delivery checkpoint is not delivery or doesn't exist");
            var res = _dbSet.Add(new CustomerOrder()
            {
                CustomerId = customerID,
                ProductId = productId,
                DeliveryPointId = deliveryPointID,
                DeliveryPoint = deliveryPoint,
                TrackNumber = trackNumber,
                Status = CustomerOrder.InitialStatus,
            });
            //вставляет status хотя не должен 
            await _context.SaveChangesAsync();
            return res.Entity;
        }


        public async Task<CustomerOrder?> GetOrderInfo(string trackNumber)
        {
            var res = await _dbSet
                .AsNoTracking()
                .Include(o => o.DeliveryPoint)
                .FirstOrDefaultAsync(x => x.TrackNumber.Equals(trackNumber));
            return res;
        }

        public async Task<CustomerOrder?> GetOrderInfo(int orderId)
        {
            if (!IsIdValid(orderId))
                return null;
            var res = await _dbSet
                .AsNoTracking()
                .Include(o => o.DeliveryPoint)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);
            return res;
        }
        /// <summary>
        /// Performance issues!
        /// </summary>
        /// <returns></returns>
        public async Task<List<CustomerOrder>> ListOrders()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<bool> UpdateOrderInfo(int id, OrderStatus status)
        {
            if(!IsIdValid(id))
                throw new ArgumentException($"Order id {id} was invalid"); 
            var order = await _dbSet
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
                throw new NotFoundException($"No order with id ${id}");
            order.Status = status;
            return true;
        }

        public async Task<List<CustomerOrder>> GetUserOrders(int userId)
        {
            if (!IsIdValid(userId))
                return [];
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.DeliveryPoint)
                .Where(c => c.CustomerId == userId)
                .ToListAsync();  
        }

        public async Task<CustomerOrder?> GetOrderById(string trackNumber)
        {
            var order = await _dbSet
                .SingleOrDefaultAsync(o => o.TrackNumber.Equals(trackNumber));
            if(order == null)
                return null;
            return order;   
        }
    }
}
