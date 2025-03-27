using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Services
{
    public interface IOrderService
    {
        Task<string> CreateOrder(int customerID, int productID, int deliverypointID, int productOwnerId);
        Task<List<OrderTrackingInfo>> GetUsersOrders(int userId);
        Task<OrderTrackingInfo> GetTrackingInfo(string trackNumber);
        Task<OrderInfo> GetOrderInfo(string trackNumber);
        Task<CustomerOrder> GerOrderInfo(string trackNumber);
        Task<bool> UpdateOrder(OrderUpdateInfo info);
        Task<List<CustomerOrder>> ListOrders();
    }
}