using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Services
{
    public interface IOrderService
    {
        Task<string> CreateOrder(int customerID, int productID, int deliverypointID, int supplierId);
        Task<OrderInfoForClient> GetOrderInfoForClient(string trackNumber);
        Task<OrderInfo> GetOrderInfo(string trackNumber);
        Task<bool> UpdateOrder(OrderUpdateInfo info);
        Task<List<CustomerOrder>> ListOrders();
    }
}