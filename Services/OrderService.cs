using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using StackExchange.Redis;
using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.SupportTools.Kafka;
using WebMarket.OrderService.SupportTools.MapSupport;
using WebMarket.OrderService.SupportTools.TrackNumber;

namespace WebMarket.OrderService.Services
{
    public class OrderService : BaseService, IOrderService
    {
        private const string OrderUpdatedTopic = "order_update";
        private readonly IOrderRepository _orderRepository;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly ITrackNumberGenerator _trackNumberGenerator;
        private readonly IKafkaMessageProducer _producer;
        private readonly IMapGeocoder _geocoder;
        private readonly IDatabase _redis;
        private readonly ILogger _logger;
        public OrderService(IOrderRepository orderRepository,  ITrackNumberGenerator trackNumberGenerator,
            ICheckpointRepository checkpointRepository,
            IKafkaMessageProducer messageProducer,
            IMapGeocoder geocoder,
            IConnectionMultiplexer connection,
            ILogger<OrderService> logger
            )   
        {
            _orderRepository = orderRepository;
            _trackNumberGenerator = trackNumberGenerator;
            _checkpointRepository = checkpointRepository;
            _producer = messageProducer;
            _geocoder = geocoder;
            _redis = connection.GetDatabase();
            _logger = logger;
        }

        private async Task<bool> SetIdForTrackNumber(string trackNumber, int id)
        {
            _logger.LogDebug("Redis: setting track number for id {Id}", id);
            return await _redis.StringSetAsync(trackNumber, id);
        }

        private async Task<int?> GetIdFromRedis(string trackNumber)
        {
            _logger.LogDebug("Redis: Requesting track number");
            var redisVal = await _redis.StringGetAsync(trackNumber);
            if (redisVal.HasValue && redisVal.IsInteger)
            {
                redisVal.TryParse(out int id);
                return id;
            }
            return null;
        }

        public async Task<CustomerOrder> GetOrderFullInfo(string trackNumber)
        {
            int? id = await GetIdFromRedis(trackNumber);
            CustomerOrder? order;
            if (id.HasValue)
                order = await _orderRepository.GetOrderInfo(id.Value);
            else
            {
                order = await _orderRepository.GetOrderInfo(trackNumber);
                if (order != null)
                {
                    await SetIdForTrackNumber(trackNumber, order.OrderId);
                }
            }
            if (order == null)
                throw new NotFoundException($"Failed to find order with track number: {trackNumber}");
            return order;
        }

        private async Task SendOrderUpdatedEvent(OrderTrackingInfo info)
        {
            await _producer.ProduceMessage(OrderUpdatedTopic, info.UserId.ToString(), JsonConvert.SerializeObject(info));
        } 

        private async Task<Checkpoint?> GetClosest(int deliverypointID, int productOwnerId)
        {
            var deliveryCheckpoint = await _checkpointRepository.GetById(deliverypointID);
            if (deliveryCheckpoint == null)
                throw new InvalidArgumentException($"Checkpoint with id {deliverypointID} not found");
            var suppliersCheckpoints = await _checkpointRepository.GetCheckpointsIdByOwner(productOwnerId);
            if (suppliersCheckpoints.Count == 0) // copilot says it's more efficient than Any()
                throw new NotFoundException("No checkpoints for given supplier " + productOwnerId);
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
            var closestSupplier = await GetClosest(deliverypointID, productOwnerId);
            if(closestSupplier == null)
                throw new NotFoundException($"Failed to find closest checkpoints. Delivery: {deliverypointID} Supplier: {productOwnerId}");

            var trackNum = _trackNumberGenerator.GenerateTrackNumber();
            var createdOrder = await _orderRepository.CreateOrder(customerID, productID, deliverypointID, closestSupplier.CheckpointId, trackNum);
            if (createdOrder == null)
                throw new PrivateServerException($"Created null order ?? cust: {customerID}, prod: {productID}, deliv: {deliverypointID}, track: {trackNum}");

            await SetIdForTrackNumber(trackNum, createdOrder.OrderId);
            await SendOrderUpdatedEvent(await CreateTrackingInfo(createdOrder));
            return createdOrder.TrackNumber;
        }


        public async Task<OrderInfo> GetOrderInfo(string trackNumber)
        {
            return await GetOrderFullInfo(trackNumber);
        }

        public async Task<OrderTrackingInfo> GetTrackingInfo(string trackNumber)
        {
            var order = await GetOrderFullInfo(trackNumber);
            return await CreateTrackingInfo(order);
        }

        private async Task<OrderTrackingInfo> CreateTrackingInfo(OrderInfo order)
        {
            string currentPos = await _geocoder.GetAddressByLongLat(order.Checkpoint.Location);
            string deliveryPos = await _geocoder.GetAddressByLongLat(order.DeliveryPoint.Location);

            return new OrderTrackingInfo(order.UserId, order.TrackNumber, currentPos, deliveryPos, order.Status);
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

        public async Task<bool> UpdateOrder(OrderUpdateInfo info)
        {
            var order = await GetOrderFullInfo(info.TrackNumber);
            var report = await _orderRepository.UpdateOrderInfo(order,info);
            if (report.Changed)
            {
               OrderTrackingInfo trackingInfo = await CreateTrackingInfo(report.OrderInfo);
               await SendOrderUpdatedEvent(trackingInfo);
            }
            return report.Changed;
        }

    }
}
