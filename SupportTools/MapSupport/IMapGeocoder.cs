using NetTopologySuite.Geometries;

namespace WebMarket.OrderService.SupportTools.MapSupport
{
    public interface IMapGeocoder
    {
        Task<string> GetAddressByLongLat(Point point);
        Task<string> GetAddressByLongLat(double longitude, double latitude);
        Task<string> GetLongLatByAddress(string address);
    }
}
