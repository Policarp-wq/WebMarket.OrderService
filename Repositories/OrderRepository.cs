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

        public async Task<CustomerOrder> CreateOrder(int customerID, int productId, int deliveryPointID, int supplyCheckpointId, string trackNumber)
        {
            if (deliveryPointID == supplyCheckpointId)
                throw new ArgumentException($"Can't create order with deliveryPoint" +
                    $" being same as supply: {deliveryPointID} - {supplyCheckpointId}");
            var deliveryPoint = await _context.Checkpoints.FindAsync(deliveryPointID);
            if (deliveryPoint == null || !deliveryPoint.IsDeliveryPoint)
                throw new ArgumentException($"Provided delivery checkpoint is not delivery or doesn't exist");
            var res = await _dbSet.AddAsync(new CustomerOrder()
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
            var created = await _dbSet.AsNoTracking()
                .Include(o => o.DeliveryPoint)
                .SingleAsync(x => x.OrderId == res.Entity.OrderId);
            //auto add chars
            return created;
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

        public async Task<List<CustomerOrder>> ListOrders()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<OrderUpdateReport> UpdateOrderInfo(OrderUpdateInfo info)
        {
            var order = await _dbSet
                .Include(o => o.DeliveryPoint)
                .FirstOrDefaultAsync(o => o.TrackNumber.Equals(info.TrackNumber));
            if (order == null)
                throw new NotFoundException($"Failed to find order with track number: {info.TrackNumber}");
            return await UpdateOrderInfo(order, info);
        }
        public async Task<OrderUpdateReport> UpdateOrderInfo(int id, OrderUpdateInfo info)
        {
            if(!IsIdValid(id))
                throw new ArgumentException($"StoryId {id} was invalid"); 
            var order = await _dbSet
                .Include(o => o.DeliveryPoint)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
                throw new NotFoundException($"Failed to find order with track number: {info.TrackNumber}");
            return await UpdateOrderInfo(order, info);
        }
        //requires trackable order!
        private async Task<OrderUpdateReport> UpdateOrderInfo(CustomerOrder order, OrderUpdateInfo info)
        {
            throw new NotImplementedException();
           
            
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

        public async Task<List<int>> GetSupplierProcessingOrders(int supplierId)
        {
            throw new NotImplementedException();
           
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
