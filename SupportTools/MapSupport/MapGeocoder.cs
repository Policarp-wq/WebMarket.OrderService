
namespace WebMarket.OrderService.SupportTools.MapSupport
{
    public class MapGeocoder : IMapGeocoder
    {
        public Task<string> GetAddressByLongLat(double longitude, double latitude)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLongLatByAddress(string address)
        {
            throw new NotImplementedException();
        }
    }
}
