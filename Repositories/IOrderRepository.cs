using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public interface IOrderRepository
    {
        Task<CustomerOrder> CreateOrder(int customerID, int productID, int deliverypointID, string trackNumber);
        Task<CustomerOrder?> GetOrderInfo(string trackNumber);
        Task<CustomerOrder?> GetOrderInfo(int orderId);
        Task<OrderUpdateReport> UpdateOrderInfo(OrderUpdateInfo info);
        Task<OrderUpdateReport> UpdateOrderInfo(int id, OrderUpdateInfo info);
        // Task<OrderUpdateReport> UpdateOrderInfo(CustomerOrder order, OrderUpdateInfo info);
        Task<List<CustomerOrder>> GetUserOrders(int userid);
        Task<List<int>> GetSupplierProcessingOrders(int supplierId);
        Task<List<CustomerOrder>> ListOrders();
        Task<CustomerOrder?> GetOrderById(string trackNumber);
    }
}
