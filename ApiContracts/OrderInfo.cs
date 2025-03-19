using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.ApiContracts
{
    public record OrderInfo(int UserId, CheckpointInfo Checkpoint, CheckpointInfo DeliveryPoint, CustomerOrder.OrderStatus Status, string TrackNumber)
    {
        public static implicit operator OrderInfo(CustomerOrder order)
        {
            return new OrderInfo(order.CustomerId, order.Checkpoint, order.DeliveryPoint, order.Status, order.TrackNumber);
        }
    }
}
