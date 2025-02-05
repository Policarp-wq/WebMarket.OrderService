using System;
using System.Collections.Generic;

namespace WebMarket.OrderService.Models;

public partial class CustomerOrder
{

    public enum OrderStatus
    {
        Processing, Packing_up, Delivering, Delivered, Completed, Denied
    }

    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int DeliveryPointId { get; set; }

    public int CheckpointId { get; set; }
    public OrderStatus Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Checkpoint Checkpoint { get; set; } = null!;

    public virtual Checkpoint DeliveryPoint { get; set; } = null!;
}
