namespace WebMarket.OrderService.SupportTools.MapSupport
{
    public interface IMapGeocoder
    {
        Task<string> GetAddressByLongLat(double longitude, double latitude);
        Task<string> GetLongLatByAddress(string address);
    }
}
