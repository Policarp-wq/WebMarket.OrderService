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
using WebMarket.OrderService.SupportTools.TrackNumber;

namespace WebMarket.OrderService.Services
{
    public class OrderService : BaseService, IOrderService
    {
        private const string OrderUpdatedTopic = "order_update";
        private const string OrderDeliveredTopic = "order_delivered";
        private const string OrderCreatedTopic = "order_created";
        private readonly IOrderRepository _orderRepository;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly ITrackNumberService _trackNumberService;
        private readonly IKafkaMessageProducer _producer;
        private readonly IMapGeocoder _geocoder;
        private readonly IRedisHandler _redisHandler;
        public OrderService(IOrderRepository orderRepository,  ITrackNumberService trackNumberService,
            ICheckpointRepository checkpointRepository,
            IKafkaMessageProducer messageProducer,
            IMapGeocoder geocoder,
            IConnectionMultiplexer connection,
            ILogger<OrderService> logger,
            IRedisHandler redisHandler
            )   
        {
            _orderRepository = orderRepository;
            _trackNumberService = trackNumberService;
            _checkpointRepository = checkpointRepository;
            _producer = messageProducer;
            _geocoder = geocoder;
            _redisHandler = redisHandler;
        }


        public async Task<CustomerOrder> GerOrderInfo(string trackNumber)
        {
            try
            {
                int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
                return (await _orderRepository.GetOrderInfo(id))!;
            }
            catch (ArgumentException ex)
            {
                throw new NotFoundException($"Failed to find order with track number: {trackNumber}");
            }
        }

        private Task<DeliveryResult<string, string>> SendOrderUpdatedEvent(OrderTrackingInfo info)
        {
            return _producer.ProduceMessage(OrderUpdatedTopic, info.UserId.ToString(), JsonConvert.SerializeObject(info));
        }
        private Task<DeliveryResult<string, string>> SendOrderCreatedEvent(CustomerOrder customerOrder, int supplierId)
        {
            return _producer.ProduceMessage(OrderCreatedTopic, supplierId.ToString(), customerOrder.OrderId.ToString());
        }
        private Task<DeliveryResult<string, string>> SendOrderDeliveredEvent(OrderTrackingInfo info)
        {
            return _producer.ProduceMessage(OrderDeliveredTopic, info.UserId.ToString(), JsonConvert.SerializeObject(info));
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
            //var closestSupplier = await GetClosest(deliverypointID, productOwnerId);
            //if(closestSupplier == null)
            //    throw new NotFoundException($"Failed to find closest checkpoints. Delivery: {deliverypointID} Supplier: {productOwnerId}");

            var trackNum = _trackNumberService.GetTrackNumber();
            // TODO: tracknumber repeat no protection
            var createdOrder = await _orderRepository.CreateOrder(customerID, productID, deliverypointID, trackNum);
            if (createdOrder == null)
                throw new PrivateServerException($"Created null order ?? cust: {customerID}, prod: {productID}, deliv: {deliverypointID}, track: {trackNum}");
            //await SetIdForTrackNumber(trackNum, createdOrder.OrderId);
            //await SendOrderCreatedEvent(createdOrder, productOwnerId);
            //await SendOrderUpdatedEvent(await CreateTrackingInfo(createdOrder));
            return trackNum;
        }

        private async Task SetIdForTrackNumber(string trackNum, int orderId)
        {
           await _redisHandler.Save(trackNum, orderId.ToString());
        }

        public async Task<OrderInfo> GetOrderInfo(int id)
        {
            return await GetOrderInfo(id);
        }

        public async Task<OrderInfo> GetOrderInfo(string trackNumber)
        {
            int id = await _trackNumberService.GetOrderIdByTrackNumber(trackNumber);
            return await GetOrderInfo(id);
        }

        public async Task<OrderTrackingInfo> GetTrackingInfo(string trackNumber)
        {
            var order = await GerOrderInfo(trackNumber);
            return await CreateTrackingInfo(order);
        }

        private async Task<OrderTrackingInfo> CreateTrackingInfo(OrderInfo order)
        {
            throw new NotImplementedException();
            //string currentPos = await _geocoder.GetAddressByLongLat(order.Checkpoint.Location);
            string deliveryPos = await _geocoder.GetAddressByLongLat(order.DeliveryPoint.Location);

            return new OrderTrackingInfo(order.OrderId, order.UserId, order.TrackNumber, "No info", deliveryPos, order.Status);
        }
        // WHEN ALL !!!
        public async Task<List<OrderTrackingInfo>> GetUsersOrders(int userId)
        {
            var orders = await _orderRepository.GetUserOrders(userId);
            var tasks = orders.Select(o => CreateTrackingInfo(o));
            return [.. (await Task.WhenAll(tasks))];
        }

        public async Task<List<CustomerOrder>> ListOrders()
        {
            return await _orderRepository.ListOrders();
        }
        //TODO: Redis not using tracknumb=id
        public async Task<bool> UpdateOrder(OrderUpdateInfo info)
        {
            throw new NotImplementedException();
            
        }

        public async Task<List<int>> GetSupplierProcessingOrders(int supplierId)
        {
            return await _orderRepository.GetSupplierProcessingOrders(supplierId);
        }
    }
}
