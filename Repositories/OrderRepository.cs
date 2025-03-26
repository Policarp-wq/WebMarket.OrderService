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
                CheckpointId = supplyCheckpointId,
                DeliveryPointId = deliveryPointID,
                DeliveryPoint = deliveryPoint,
                TrackNumber = trackNumber,
                Status = CustomerOrder.OrderStatus.Processing,
            });
            //вставляет status хотя не должен 
            await _context.SaveChangesAsync();
            var created = await _dbSet.AsNoTracking()
                .Include(o => o.Checkpoint)
                .Include(o => o.DeliveryPoint)
                .SingleAsync(x => x.OrderId == res.Entity.OrderId);
            //auto add chars
            return created;
        }


        public async Task<CustomerOrder?> GetOrderInfo(string trackNumber)
        {
            var res = await _dbSet
                .AsNoTracking()
                .Include(o => o.Checkpoint)
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
                .Include(o => o.Checkpoint)
                .Include(o => o.DeliveryPoint)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);
            return res;
        }

        public async Task<List<CustomerOrder>> ListOrders()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<OrderUpdateReport?> UpdateOrderInfo(OrderUpdateInfo info)
        {
            var order = await _dbSet
                .Include(o => o.Checkpoint)
                .Include(o => o.DeliveryPoint)
                .FirstOrDefaultAsync(o => o.TrackNumber.Equals(info.TrackNumber));
            if (order == null)
                throw new NotFoundException($"Failed to find order with track number: {info.TrackNumber}");
            return await UpdateOrderInfo(order, info);
        }
        public async Task<OrderUpdateReport> UpdateOrderInfo(CustomerOrder order, OrderUpdateInfo info)
        {
            if (order.TrackNumber != info.TrackNumber)
                throw new InvalidArgumentException($"TrackNumber from order {order.TrackNumber} is different from given info {info.TrackNumber}");
            bool updated = false;
            var stategy = _context.Database.CreateExecutionStrategy();
            return await stategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                if (info.Status != null && order.Status != info.Status)
                {
                    order.Status = info.Status!.Value;
                    updated = true;
                }
                if (info.CheckpointID != null && info.CheckpointID != order.CheckpointId)
                {
                    order.CheckpointId = info.CheckpointID!.Value;
                    order.Checkpoint = await _context.Checkpoints.FindAsync(order.CheckpointId)
                        ?? throw new NotFoundException($"Checkpoint not found for  id: {order.CheckpointId}");
                    if(order.CheckpointId == order.DeliveryPointId)
                        order.Status = CustomerOrder.OrderStatus.Delivered;
                    updated = true;
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new PrivateServerException("Aborted saving order with given data", ex);
                }
                var report = new OrderUpdateReport(updated, order.CustomerId, (OrderInfo)order);
                await transaction.CommitAsync();
                return report;
            });
            
        }

        public async Task<List<CustomerOrder>> GetUserOrders(int userId)
        {
            if (!IsIdValid(userId))
                return [];
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.Checkpoint)
                .Include(x => x.DeliveryPoint)
                .Where(c => c.CustomerId == userId)
                .ToListAsync();  
        }
    }
}
