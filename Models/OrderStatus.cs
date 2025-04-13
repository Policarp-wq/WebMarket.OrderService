using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace WebMarket.OrderService.Models;

public enum OrderStatus
{
    Processing, Packing_up, Delivering, Delivered, Completed, Denied
}
