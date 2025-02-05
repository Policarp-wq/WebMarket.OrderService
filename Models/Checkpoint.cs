using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMarket.OrderService.Models;

public partial class Checkpoint
{
    public int CheckpointId { get; set; }

    public int OwnerId { get; set; }
    public Point Location { get; set; } = null!;

    public virtual ICollection<CustomerOrder> CustomerOrderCheckpoints { get; set; } = new List<CustomerOrder>();

    public virtual ICollection<CustomerOrder> CustomerOrderDeliveryPoints { get; set; } = new List<CustomerOrder>();
}
