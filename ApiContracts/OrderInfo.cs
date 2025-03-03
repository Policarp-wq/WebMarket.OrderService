using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.ApiContracts
{
    public record OrderInfo(int CheckpointId, int DeliveryPointId, CustomerOrder.OrderStatus Status, string TrackNumber)
    {
        public static explicit operator OrderInfo(CustomerOrder order)
        {
            return new OrderInfo(order.CheckpointId, order.DeliveryPointId, order.Status, order.TrackNumber);
        }
    }
}
