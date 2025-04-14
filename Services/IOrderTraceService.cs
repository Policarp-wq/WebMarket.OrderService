using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Services
{
    public interface IOrderTraceService
    {
        public Task<OrderTraceRoute> GetOrderRoute(string trackNumber);
        public Task AddRouteUnit(string trackNumber, int checkpointId);
        public Task<bool> UpdateLastTraceInfo(string trackNumber, DeliveryStatus status);
        public Task<bool> SetOrderDelivered(string trackNumber, DateTime deliveredTime);
    }
}
