
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
    public bool IsDeliveryPoint { get; set; }

    public virtual ICollection<CustomerOrder> CustomerOrderCheckpoints { get; set; } = new List<CustomerOrder>();

    public virtual ICollection<CustomerOrder> CustomerOrderDeliveryPoints { get; set; } = new List<CustomerOrder>();
    /// <summary>
    ///  
    /// </summary>
    /// <param name="checkpoints"></param>
    /// <param name="excludeSelf"></param>
    /// <returns> Return closest checkpoint from given,
    /// if given enum is empty or enum contains only current checkpoint
    /// and excludeSelf is true, then returns null</returns>
    public Checkpoint? FindClosest(IEnumerable<Checkpoint> checkpoints, bool excludeSelf = true)
    {
        if(!checkpoints.Any()) 
            return null;
        Checkpoint? result = null;
        double minDist = int.MaxValue;
        foreach (var checkpoint in checkpoints)
        {
            var dist = checkpoint.Location.Distance(Location);
            if (dist < minDist)
            {
                if(checkpoint.CheckpointId == CheckpointId && excludeSelf)
                    continue;
                result = checkpoint;
                minDist = dist;
            }
        }
        return result;
    }
}
