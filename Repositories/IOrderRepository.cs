using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public interface IOrderRepository
    {
        Task<int> CreateOrder(int customerID, int productID, int checkpointID);
        Task<OrderInfo> GetOrderInfo(int trackNumber);
        Task<bool> UpdateOrderInfo(int CheckpointId, CustomerOrder.OrderStatus status);
    }
}
