namespace WebMarket.OrderService.Models
{
    public class OrderTrace
    {
        public int TraceId {  get; set; }
        public int OrderId { get; set; }
        public int CheckpointId { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public virtual CustomerOrder CustomerOrder { get; set; } = null!;
        public virtual Checkpoint Checkpoint { get; set; } = null!;
        public static readonly DeliveryStatus InitStatus = DeliveryStatus.Delivering_to; 
    }
}
