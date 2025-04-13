
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
        public TrackNumberService(ITrackNumberGenerator generator, IOrderRepository orderRepository, IRedisHandler handler) 
        {
            _trackNumberGenerator = generator;
            _orderRepository = orderRepository;
            _redis = handler;
        }
        public async Task<int> GetOrderIdByTrackNumber(string trackNumber)
        {
            var id = await _redis.GetInt(trackNumber);
            if (id.HasValue)
                return id.Value;
            var order = await _orderRepository.GetOrderById(trackNumber);
            if (order != null)
            {
                await _redis.Save(trackNumber, order.OrderId.ToString());
                return id.Value;
            }
            throw new ArgumentException("No order id for tracknumber {tracknumber}", trackNumber);
        }

        public string GetTrackNumber()
        {
            var trackNumber = _trackNumberGenerator.GenerateTrackNumber();
            return trackNumber;
        }
    }
}
