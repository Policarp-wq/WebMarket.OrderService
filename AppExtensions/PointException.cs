using NetTopologySuite.Geometries;

namespace WebMarket.OrderService.AppExtensions
{
    public static class PointException
    {
        private static bool IsGeoCoordValid(double c) => c <= 180 && c >= -180;
        public static bool IsGeo(this Point point)
        {
            return IsGeoCoordValid(point.X) && IsGeoCoordValid(point.Y);
        }
    }
}
