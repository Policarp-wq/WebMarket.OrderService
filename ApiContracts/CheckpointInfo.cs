using NetTopologySuite.Geometries;
using System.Text.Json.Serialization;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.SupportTools;

namespace WebMarket.OrderService.ApiContracts
{
    public record CheckpointInfo
    {
        [JsonConverter(typeof(PointJsonConverter))]
        public Point Point { get; set; }
        public int OwnerId { get; set; }
        public CheckpointInfo(Point Point, int OwnerId)
        {
            this.Point = Point;
            this.OwnerId = OwnerId;
        }
        public static implicit operator CheckpointInfo(Checkpoint checkpoint)
        {
            return new CheckpointInfo(checkpoint.Location, checkpoint.OwnerId);
        }
    }
}
