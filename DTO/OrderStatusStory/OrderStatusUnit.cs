using System.Text.Json.Serialization;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.DTO.OrderStatusStory
{
    public class OrderStatusUnit
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }
        public DateTime ChangeDate { get; set; }

        public OrderStatusUnit(OrderStatus status, DateTime changeDate)
        {
            Status = status;
            ChangeDate = changeDate;
        }
    }

    public record OrderStatusStoryForClient(int OrderId, IEnumerable<OrderStatusUnit> Units);
}