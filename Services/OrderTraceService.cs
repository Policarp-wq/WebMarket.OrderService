
using StackExchange.Redis;
using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Models;
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

        public async Task<bool> AddRouteUnit(string trackNumber, int checkpointId)
        {
            int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
            return await _traceRepository.AddRouteUnit(id, checkpointId);
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

        public async Task<bool> SetOrderDelivered(string trackNumber, DateTime deliveredTime)
        {
            int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
            return await _traceRepository.SetOrderDeliveredTime(id, deliveredTime);
        }

        public async Task<bool> UpdateLastTraceInfo(string trackNumber, DeliveryStatus status)
        {
            int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
            return await _traceRepository.UpdateLastTraceInfo(id, status);
        }
    }
}
