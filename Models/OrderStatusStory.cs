using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebMarket.OrderService.Models;

public partial class OrderStatusStory
{
    public int StoryId { get; set; }
    public int OrderId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus Status { get; set; }

    public DateTime ChangeDate { get; set; }
}
