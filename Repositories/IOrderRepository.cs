using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderInfo> CreateOrder(int customerID, int productID, int deliverypointID, int supplierID, string trackNumber);
        Task<OrderInfo> GetOrderInfo(string trackNumber);
        Task<OrderUpdateReport> UpdateOrderInfo(UpdateOrderInfo info);
        Task<List<CustomerOrder>> ListOrders();
    }
}
