using WebMarket.OrderService.DTO.OrderStatusStory;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;

namespace WebMarket.OrderService.Services
{
    public class OrderStatusStoryService : BaseService, IOrderStatusStoryService
    {
        private readonly IOrderStatusStoryRepository _storyRepository;
        private readonly ITrackNumberService _trackNumberService;

        public OrderStatusStoryService(IOrderStatusStoryRepository storyRepository, ITrackNumberService trackNumberService)
        {
            _storyRepository = storyRepository;
            _trackNumberService = trackNumberService;
        }

        public async Task<OrderStatusUnit> GetLatestStatus(string trackNumber)
        {
            int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
            return await _storyRepository.GetLatestStatus(id);
        }

        public async Task<OrderStatusStoryForClient> GetOrderStatusStory(string trackNumber)
        {
            int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
            var units = await _storyRepository.GetOrderStory(id);
            return new OrderStatusStoryForClient(id, units);
        }

        public async Task<bool> UpdateStatus(string trackNumber, OrderStatus status)
        {
            int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
            return await _storyRepository.UpdateStatus(id, status);
        }
    }
}
