using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.ApiContracts
{
    public record OrderInfo(Checkpoint Checkpoint, Checkpoint DeliveryPoint, CustomerOrder.OrderStatus Status, string TrackNumber)
    {
        public static explicit operator OrderInfo(CustomerOrder order)
        {
            return new OrderInfo(order.Checkpoint, order.DeliveryPoint, order.Status, order.TrackNumber);
        }
    }
}
