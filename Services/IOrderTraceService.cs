using WebMarket.OrderService.DTO.Order;

namespace WebMarket.OrderService.Services
{
    public interface IOrderTraceService
    {
        public Task<OrderTraceRoute> GetOrderRoute(int orderId);
        public Task<OrderTraceRoute> GetOrderRoute(string trackNumber);
    }
}
