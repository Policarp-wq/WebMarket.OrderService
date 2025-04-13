using WebMarket.OrderService.DTO.Checkpoints;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.DTO.Order
{
    public record OrderInfo(int OrderId, int UserId, CheckpointInfo DeliveryPoint, OrderStatus Status, string TrackNumber)
    {
        public static implicit operator OrderInfo(CustomerOrder order)
        {
            return new OrderInfo(order.OrderId, order.CustomerId, order.DeliveryPoint, order.Status, order.TrackNumber);
        }
    }
}
