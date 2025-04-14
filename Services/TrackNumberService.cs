
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.SupportTools.Redis;
using WebMarket.OrderService.SupportTools.TrackNumber;

namespace WebMarket.OrderService.Services
{
    public class TrackNumberService : BaseService, ITrackNumberService
    {
        private readonly ITrackNumberGenerator _trackNumberGenerator;
        private readonly IOrderRepository _orderRepository;
        private readonly IRedisHandler _redis;
        public static string GetRedisTrackKey(string trackNumber) => $"track:{trackNumber}";
        public static readonly TimeSpan TrackNumberExpirationDate = TimeSpan.FromDays(5);
        public TrackNumberService(ITrackNumberGenerator generator, IOrderRepository orderRepository, IRedisHandler handler) 
        {
            _trackNumberGenerator = generator;
            _orderRepository = orderRepository;
            _redis = handler;
        }
        public async Task<int> GetOrderIdByTrackNumber(string trackNumber)
        {
            var id = await _redis.GetInt(GetRedisTrackKey(trackNumber));
            if (id.HasValue)
                return id.Value;
            var order = await _orderRepository.GetOrderInfo(trackNumber);
            if (order != null)
            {
                await _redis.Save(trackNumber, order.OrderId.ToString());
                return id.Value;
            }
            throw new ArgumentException("No order id for trackNumber {tracknumber}", trackNumber);
        }
        public async Task<bool> CacheTrackNumber(int orderId, string trackNumber)
        {
            return await _redis.Save(GetRedisTrackKey(trackNumber), orderId, TrackNumberExpirationDate);
        }

        public string GetTrackNumber()
        {
            var trackNumber = _trackNumberGenerator.GenerateTrackNumber();
            return trackNumber;
        }
    }
}
