using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public interface IOrderTraceRepository
    {
        public Task<IEnumerable<OrderTraceRouteUnit>> GetOrderRoute(int OrderId);
        public Task<bool> AddRouteUnit(int orderId, int checkpointId);
        public Task<bool> UpdateLastTraceInfo(int orderId, DeliveryStatus status);
        public Task<bool> SetOrderDeliveredTime(int orderId, DateTime deliveredTime);  
    }
}
