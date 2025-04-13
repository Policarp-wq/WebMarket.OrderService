namespace WebMarket.OrderService.DTO.Order
{
    public record OrderTraceRouteUnit(string? Address, DateTime DeliveryDate);
    public record OrderTraceRoute(int OrderId, IEnumerable<OrderTraceRouteUnit> Units);
}
