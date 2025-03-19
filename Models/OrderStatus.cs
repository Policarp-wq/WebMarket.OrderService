namespace WebMarket.OrderService.Models;

public partial class CustomerOrder
{
    public enum OrderStatus
    {
        Processing, Packing_up, Delivering, Delivered, Completed, Denied
    }
}
