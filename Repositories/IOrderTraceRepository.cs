using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public interface IOrderTraceRepository
    {
        public Task<IEnumerable<OrderTraceRouteUnit>> GetOrderRoute(int OrderId);

    }
}
