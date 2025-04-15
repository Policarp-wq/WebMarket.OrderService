using WebMarket.OrderService.DTO.Checkpoints;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.DTO.Order
{
    public record OrderInfoForCustomer(CheckpointInfo DeliveryPoint, OrderStatus Status, string TrackNumber)
    {
        public static explicit operator OrderInfoForCustomer(CustomerOrder order)
        {
            return new OrderInfoForCustomer((CheckpointInfo)order.DeliveryPoint, order.Status,
                order.TrackNumber);
        }
    }
}
