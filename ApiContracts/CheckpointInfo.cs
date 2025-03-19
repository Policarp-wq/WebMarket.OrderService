using NetTopologySuite.Geometries;
using System.Text.Json.Serialization;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.SupportTools;

namespace WebMarket.OrderService.ApiContracts
{
    public record CheckpointInfo
    {
        public int CheckpointId { get; set; }
        [JsonConverter(typeof(PointJsonConverter))]
        public Point Location { get; set; }
        public int OwnerId { get; set; }
        public CheckpointInfo(int CheckpointId, Point Point, int OwnerId)
        {
            this.CheckpointId = CheckpointId;
            this.Location = Point;
            this.OwnerId = OwnerId;
        }
        public static implicit operator CheckpointInfo(Checkpoint checkpoint)
        {
            return new CheckpointInfo(checkpoint.CheckpointId, checkpoint.Location, checkpoint.OwnerId);
        }
    }
}
