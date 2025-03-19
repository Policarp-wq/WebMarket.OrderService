using System.Text.Json;
using System.Text.Json.Serialization;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.ApiContracts
{
    public class OrderTrackingInfo
    {
        public int UserId { get; }
        public string TrackNumber { get; }
        public string CurrentAddress { get; }
        public string DeliveryAddress { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))] // when adding this converter all works everywhere wtf
        public CustomerOrder.OrderStatus Status { get; }

        public OrderTrackingInfo(int userId, string trackNumber, string currentAddress, string deliveryAddress, CustomerOrder.OrderStatus status)
        {
            UserId = userId;
            TrackNumber = trackNumber;
            CurrentAddress = currentAddress;
            DeliveryAddress = deliveryAddress;
            Status = status;
        }
    }
}
