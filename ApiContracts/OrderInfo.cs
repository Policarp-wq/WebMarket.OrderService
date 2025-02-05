using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.ApiContracts
{
    public record OrderInfo(string Checkpoint, string DeliveryPoint, CustomerOrder.OrderStatus Status)
    {
        public static explicit operator OrderInfo(CustomerOrder order)
        {
            throw new NotImplementedException();
        }
    }
}
