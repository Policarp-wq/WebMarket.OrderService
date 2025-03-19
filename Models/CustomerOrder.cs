using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebMarket.OrderService.Models;

public partial class CustomerOrder
{

    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int DeliveryPointId { get; set; }

    public int CheckpointId { get; set; }
    public string TrackNumber { get; set; } = null!;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual Checkpoint Checkpoint { get; set; } = null!;

    public virtual Checkpoint DeliveryPoint { get; set; } = null!;
}
