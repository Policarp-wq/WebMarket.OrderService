using Confluent.Kafka;
using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using StackExchange.Redis;
using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.SupportTools.Kafka;
using WebMarket.OrderService.SupportTools.MapSupport;
using WebMarket.OrderService.SupportTools.Redis;

namespace WebMarket.OrderService.Services
{
    public class OrderService : BaseService, IOrderService
    {
        private const string OrderCreatedTopic = "order_created";
        private readonly IOrderRepository _orderRepository;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly ITrackNumberService _trackNumberService;
        private readonly IKafkaMessageProducer _producer;
        public OrderService(IOrderRepository orderRepository,  ITrackNumberService trackNumberService,
            ICheckpointRepository checkpointRepository,
            IKafkaMessageProducer messageProducer,
            IMapGeocoder geocoder
            )   
        {
            _orderRepository = orderRepository;
            _trackNumberService = trackNumberService;
            _checkpointRepository = checkpointRepository;
            _producer = messageProducer;
        }


        public async Task<CustomerOrder> GerOrderInfo(string trackNumber)
        {
            try
            {
                int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
                return (await _orderRepository.GetOrderInfo(id))!;
            }
            catch (ArgumentException)
            {
                throw new NotFoundException($"Failed to find order with track number: {trackNumber}");
            }
        }

        private Task<DeliveryResult<string, string>> SendOrderCreatedEvent(CustomerOrder customerOrder, int supplierId)
        {
            return _producer.ProduceMessage(OrderCreatedTopic, supplierId.ToString(), customerOrder.OrderId.ToString());
        }

        private async Task<Checkpoint?> GetClosest(int deliverypointID, int productOwnerId)
        {
            var deliveryCheckpoint = await _checkpointRepository.GetById(deliverypointID);
            if (deliveryCheckpoint == null)
                throw new NotFoundException($"Delivery point with id {deliverypointID} not found");
            var suppliersCheckpoints = await _checkpointRepository.GetCheckpointsIdByOwner(productOwnerId);
            if (suppliersCheckpoints.Count == 0) // copilot says it's more efficient than Any()
                throw new NotFoundException("No checkpoints for provided supplier " + productOwnerId);
            return deliveryCheckpoint.FindClosest(suppliersCheckpoints);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="productID"></param>
        /// <param name="deliverypointID"></param>
        /// <param name="productOwnerId"></param>
        /// <returns>Track number of the created order</returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="PrivateServerException"></exception>

        public async Task<string> CreateOrder(int customerID, int productID, int deliverypointID, int productOwnerId)
        {
            var trackNum = _trackNumberService.GetTrackNumber();
            // TODO: tracknumber repeat no protection
            var createdOrder = await _orderRepository.CreateOrder(customerID, productID, deliverypointID, trackNum);
            if (createdOrder == null)
                throw new PrivateServerException($"Created null order ?? cust: {customerID}, prod: {productID}, deliv: {deliverypointID}, track: {trackNum}");
            await SetIdForTrackNumber(trackNum, createdOrder.OrderId);
            await SendOrderCreatedEvent(createdOrder, productOwnerId);
            //await SendOrderUpdatedEvent(await CreateTrackingInfo(createdOrder));
            return trackNum;
        }

        private async Task SetIdForTrackNumber(string trackNum, int orderId)
        {
           await _trackNumberService.CacheTrackNumber(orderId, trackNum);
        }

        public async Task<OrderInfoForCustomer> GetOrderInfo(int id)
        {
            var order = await _orderRepository.GetOrderInfo(id);
            if (order == null)
                throw new NotFoundException($"No order with id {id}");
            return (OrderInfoForCustomer) order;
        }

        public async Task<OrderInfoForCustomer> GetOrderInfo(string trackNumber)
        {
            int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
            return await GetOrderInfo(id);
        }
        // WHEN ALL !!!
        public async Task<List<OrderInfoForCustomer>> GetUsersOrders(int userId)
        {
            return (await _orderRepository.GetUserOrders(userId))
                .Select(c => (OrderInfoForCustomer)c)
                .ToList();
        }

        public async Task<List<CustomerOrder>> ListOrders()
        {
            return await _orderRepository.ListOrders();
        }
    }
}
