using WebMarket.OrderService.DTO.OrderStatusStory;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public interface IOrderStatusStoryRepository
    {
        Task<bool> UpdateStatus(int orderId, OrderStatus status);
        Task<IEnumerable<OrderStatusUnit>> GetOrderStory(int orderId);
        Task<OrderStatusUnit> GetLatestStatus(int orderId);
        Task<bool> InitOrder(int orderId);
    }
}
