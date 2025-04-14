namespace WebMarket.OrderService.Services
{
    public interface ITrackNumberService
    {
        Task<int> GetOrderIdByTrackNumber(string trackNumber);
        Task<bool> CacheTrackNumber(int orderId, string trackNumber);
        string GetTrackNumber();
    }
}
