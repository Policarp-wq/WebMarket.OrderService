using System.Text.Json;
using System.Text.Json.Serialization;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.DTO.Order
{
    public class OrderTrackingInfo
    {
        [JsonIgnore]
        public int OrderId { get; set; }
        public int UserId { get; }
        public string TrackNumber { get; }
        public string CurrentAddress { get; }
        public string DeliveryAddress { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))] // when adding this converter all works everywhere wtf
        public OrderStatus Status { get; }

        public OrderTrackingInfo(int orderId, int userId, string trackNumber, string currentAddress, string deliveryAddress, OrderStatus status)
        {
            OrderId = orderId;
            UserId = userId;
            TrackNumber = trackNumber;
            CurrentAddress = currentAddress;
            DeliveryAddress = deliveryAddress;
            Status = status;
        }
    }
}
