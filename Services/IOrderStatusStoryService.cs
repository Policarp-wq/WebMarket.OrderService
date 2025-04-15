using WebMarket.OrderService.DTO.OrderStatusStory;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Services
{
    public interface IOrderStatusStoryService
    {
        Task<OrderStatusStoryForClient> GetOrderStatusStory(string trackNumber);
        Task<bool> UpdateStatus(string trackNumber, OrderStatus status);
        Task<OrderStatusUnit> GetLatestStatus(string trackNumber);
    }
}
