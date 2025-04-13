using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace WebMarket.OrderService.Models;

public enum OrderStatus
{
    Processing = 1, Packing_up = 2, Delivering = 3, Delivered = 4, Completed = 5, Denied = 6
}
