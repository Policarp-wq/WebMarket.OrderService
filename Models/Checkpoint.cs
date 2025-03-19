
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebMarket.OrderService.SupportTools;

namespace WebMarket.OrderService.Models;

public partial class Checkpoint
{
    public int CheckpointId { get; set; }
    public int OwnerId { get; set; }

    [JsonConverter(typeof(PointJsonConverter))]
    public Point Location { get; set; } = null!;
    public bool IsDelivryPoint { get; set; }

    public virtual ICollection<CustomerOrder> CustomerOrderCheckpoints { get; set; } = new List<CustomerOrder>();

    public virtual ICollection<CustomerOrder> CustomerOrderDeliveryPoints { get; set; } = new List<CustomerOrder>();
}
