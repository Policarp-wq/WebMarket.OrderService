using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Services
{
    public interface IOrderService
    {
        Task<string> CreateOrder(int customerID, int productID, int deliverypointID, int productOwnerId);
        Task<List<OrderInfoForCustomer>> GetUsersOrders(int userId);
        Task<OrderInfoForCustomer> GetOrderInfo(string trackNumber);
        Task<CustomerOrder> GerOrderInfo(string trackNumber);
        Task<bool> UpdateOrder(string trackNumber, OrderStatus status);
        Task<List<CustomerOrder>> ListOrders();
    }
}