using NetTopologySuite.Geometries;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebMarket.OrderService.SupportTools
{
    public class PointJsonConverter : JsonConverter<Point>
    {
        private class SimplePoint 
        {
            public double X { get; set; }
            public double Y { get; set; }

            public SimplePoint(Point point)
            {
                X = point.X;
                Y = point.Y;
            }
        }
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string str = reader.GetString()!;
            var simple = JsonSerializer.Deserialize<SimplePoint>(str);
            if (simple is null)
                throw new JsonException($"Failed to convert ${str} to Location");
            return new Point(simple.X, simple.Y);
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            var simple = new SimplePoint(value);
            JsonSerializer.Serialize(writer, simple, options);
        }
    }
}
