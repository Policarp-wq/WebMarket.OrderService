using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebMarket.OrderService.Models;

public class CustomerOrder
{
    public static readonly OrderStatus InitialStatus = OrderStatus.Processing;
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int DeliveryPointId { get; set; }

    public string TrackNumber { get; set; } = null!;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual Checkpoint DeliveryPoint { get; set; } = null!;
    public virtual ICollection<OrderTrace> OrderTraces { get; set; } = [];
}
