using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.ApiContracts
{
    public record OrderUpdateInfo(string TrackNumber, int? CheckpointID, CustomerOrder.OrderStatus? Status)
    {
    }
}
