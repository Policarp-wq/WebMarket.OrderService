

using NetTopologySuite.Geometries;

namespace WebMarket.OrderService.ApiContracts
{
    public record LocationPresentation(double Longitude, double Latitude)
    {
        public static explicit operator Point(LocationPresentation locationPresentation)
        {
            return new Point(locationPresentation.Longitude, locationPresentation.Latitude);
        }

        public static explicit operator LocationPresentation(Point point)
        {
            return new LocationPresentation(point.X, point.Y);
        }
    }
}
