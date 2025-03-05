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

        public async Task<OrderInfo> CreateOrder(int customerID, int productId, int deliveryPointID, int supplierID, string trackNumber)
        {
            var res = await _dbSet.AddAsync(new CustomerOrder()
            {
                CustomerId = customerID,
                ProductId = productId,
                CheckpointId = supplierID,
                DeliveryPointId = deliveryPointID,
                TrackNumber = trackNumber,
                Status = CustomerOrder.OrderStatus.Processing,
            });
            //вставляет status хотя не должен 
            await _context.SaveChangesAsync();
            return (OrderInfo)res.Entity;
        }


        public async Task<OrderInfo> GetOrderInfo(string trackNumber)
        {
            var res = await _dbSet
                .AsNoTracking()
                .Include(o => o.Checkpoint)
                .Include(o => o.DeliveryPoint)
                .FirstOrDefaultAsync(x => x.TrackNumber.Equals(trackNumber));
            if (res == null)
                throw new NotFoundException("Didn't find order with given tracknumber: " + trackNumber);
            return (OrderInfo)res;
        }

        public async Task<List<CustomerOrder>> ListOrders()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<OrderUpdateReport> UpdateOrderInfo(OrderUpdateInfo info)
        {
            var order = await _dbSet
                .FirstOrDefaultAsync(o => o.TrackNumber.Equals(info.TrackNumber));
            if (order == null)
                throw new NotFoundException($"Didn't find order for update with given track number: {info.TrackNumber}");
            return GetUpdatedOrder(order, info);
        }

        private static OrderUpdateReport GetUpdatedOrder(CustomerOrder order, OrderUpdateInfo info)
        {
            bool updated = false;
            if (info.Status != null && order.Status != info.Status)
            {
                order.Status = info.Status!.Value;
                updated = true;
            }
            if (info.CheckpointID != null && info.CheckpointID != order.CheckpointId)
            {
                order.CheckpointId = info.CheckpointID!.Value;
                updated = true;
            }
            return new OrderUpdateReport(updated, order.CustomerId, (OrderInfo)order);

        }

    }
}
