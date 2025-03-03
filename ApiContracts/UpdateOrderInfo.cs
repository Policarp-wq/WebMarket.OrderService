using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.ApiContracts
{
    public record UpdateOrderInfo(int OrderID, int? CheckpointID, CustomerOrder.OrderStatus? Status)
    {
    }
}
