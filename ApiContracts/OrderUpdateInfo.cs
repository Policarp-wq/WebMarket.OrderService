using System.Text.Json.Serialization;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.ApiContracts
{
    public record OrderUpdateInfo
    {
        public string TrackNumber { get; set; }
        public int? CheckpointID {  get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CustomerOrder.OrderStatus? Status {  get; set; }
        public OrderUpdateInfo(string TrackNumber, int? CheckpointID, CustomerOrder.OrderStatus? Status)
        {
            this.TrackNumber = TrackNumber;
            this.CheckpointID = CheckpointID;
            this.Status = Status;
        }
    }
}
