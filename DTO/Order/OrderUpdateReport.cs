namespace WebMarket.OrderService.DTO.Order
{
    public record OrderUpdateReport(bool Changed, int UserId, OrderInfo OrderInfo);
}
