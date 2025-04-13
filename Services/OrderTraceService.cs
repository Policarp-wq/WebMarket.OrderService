
using StackExchange.Redis;
using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.SupportTools.Redis;

namespace WebMarket.OrderService.Services
{
    public class OrderTraceService : BaseService, IOrderTraceService
    {
        private readonly IOrderTraceRepository _traceRepository;
        private readonly ITrackNumberService _trackNumberService;   
        public OrderTraceService(IRedisHandler handler, IOrderTraceRepository traceRepository, ITrackNumberService trackNumberService) 
        {
            _traceRepository = traceRepository;
            _trackNumberService = trackNumberService;
        }
        public async Task<OrderTraceRoute> GetOrderRoute(int orderId)
        {
            var route = await _traceRepository.GetOrderRoute(orderId);
            return new OrderTraceRoute(orderId, route);
        }

        public async Task<OrderTraceRoute> GetOrderRoute(string trackNumber)
        {
            int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
            return await GetOrderRoute(id);
        }
    }
}
