namespace WebMarket.OrderService.ApiContracts
{
    public record OrderUpdateReport(bool Changed, int UserId, OrderInfo OrderInfo);
}
