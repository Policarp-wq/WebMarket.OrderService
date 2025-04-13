namespace WebMarket.OrderService.Services
{
    public interface ITrackNumberService
    {
        Task<int> GetOrderIdByTrackNumber(string trackNumber);
        string GetTrackNumber();
    }
}
