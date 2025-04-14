using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.DTO.Order
{
    public record OrderTraceRouteUnit(string? Address, DateTime? DeliveryDate, DeliveryStatus Status);
    public record OrderTraceRoute(int OrderId, IEnumerable<OrderTraceRouteUnit> Units);
}
