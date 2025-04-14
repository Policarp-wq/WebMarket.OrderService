using WebMarket.OrderService.DTO.Checkpoints;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.DTO.Order
{
    public record OrderInfoForCustomer(int OrderId, int UserId, CheckpointInfo DeliveryPoint, OrderStatus Status, string TrackNumber)
    {
        public static explicit operator OrderInfoForCustomer(CustomerOrder order)
        {
            return new OrderInfoForCustomer(order.OrderId, order.CustomerId,
                (CheckpointInfo)order.DeliveryPoint, order.Status, order.TrackNumber);
        }
    }
}
