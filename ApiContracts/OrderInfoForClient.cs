using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.ApiContracts
{
    public record OrderInfoForClient(string CurrentPosition, string DeliveryPoint, string TrackNumber, CustomerOrder.OrderStatus Status);
    
    
}
