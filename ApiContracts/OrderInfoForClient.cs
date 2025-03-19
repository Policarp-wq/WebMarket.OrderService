using System.Text.Json.Serialization;
using WebMarket.OrderService.Models;
using static WebMarket.OrderService.Models.CustomerOrder;

namespace WebMarket.OrderService.ApiContracts
{
    public record OrderInfoForClient
    {
        public readonly string CurrentPosition;
        public readonly string DeliveryPoint;
        public readonly string TrackNumber;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public readonly OrderStatus Status;
        public OrderInfoForClient(string CurrentPosition, string DeliveryPoint, string TrackNumber, CustomerOrder.OrderStatus Status)
        {
            this.CurrentPosition = CurrentPosition;
            this.DeliveryPoint = DeliveryPoint;
            this.TrackNumber = TrackNumber;
            this.Status = Status;
        }
    }
    
    
}
