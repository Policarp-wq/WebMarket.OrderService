using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Repositories;

namespace WebMarket.OrderService.Services
{
    public class OrderService : BaseService
    {
        private IOrderRepository _orderRepository;
        public OrderService(IOrderRepository orderRepository) 
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderInfo> GetOrderInfo(int trackNumber)
        {
            var info = await _orderRepository.GetOrderInfo(trackNumber);
            return info;
        }
    }
}
