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

        private static Checkpoint FindClosest(Checkpoint target, IEnumerable<Checkpoint> checkpoints)
        {
            var result = checkpoints.First();
            double minDist = target.Location.Distance(result.Location);
            foreach (var checkpoint in checkpoints)
            {
                var dist = checkpoint.Location.Distance(result.Location);
                if (dist < minDist)
                {
                    result = checkpoint;
                    minDist = dist;
                }
            }
            return result;
        }

        private async Task SendOrderUpdatedEvent(OrderTrackingInfo info)
        {
            _logger.LogDebug("Sending event to topic {Topic}", OrderUpdatedTopic);
            await _producer.ProduceMessage(OrderUpdatedTopic, info.UserId.ToString(), JsonConvert.SerializeObject(info));
            _logger.LogDebug("Event sent");
        } 


        public async Task<string> CreateOrder(int customerID, int productID, int deliverypointID, int productOwnerId)
        {
            var deliveryCheckpoint = await _checkpointRepository.GetById(deliverypointID);
            if (deliveryCheckpoint == null)
                throw new InvalidArgumentException($"Checkpoint with id {deliverypointID} not found");
            var suppliersCheckpoints = await _checkpointRepository.GetCheckpointsIdByOwner(productOwnerId);
            if (suppliersCheckpoints.Count == 0) // says it's more efficient
                throw new NotFoundException("No checkpoints for given supplier " + productOwnerId);
            var closestSupplier = FindClosest(deliveryCheckpoint, suppliersCheckpoints);

            var trackNum = _trackNumberGenerator.GenerateTrackNumber();
            var createdOrder = await _orderRepository.CreateOrder(customerID, productID, deliverypointID, closestSupplier.CheckpointId, trackNum);
            if (createdOrder == null)
                throw new PrivateServerException($"Created null order ?? cust: {customerID}, prod: {productID}, deliv: {deliverypointID}, track: {trackNum}");
            _redis.StringSet(createdOrder.TrackNumber, createdOrder.OrderId);
            SendOrderUpdatedEvent(await CreateTrackingInfo(createdOrder));
            return createdOrder.TrackNumber;
        }


        public async Task<OrderInfo> GetOrderInfo(string trackNumber)
        {
            var info = await _orderRepository.GetOrderInfo(trackNumber);
            if (info == null)
                throw new NotFoundException($"Failed to find order with track number: {trackNumber}");
            return (OrderInfo)info;
        }

        public async Task<OrderInfoForClient> GetOrderInfoForClient(string trackNumber)
        {
            var info = await GetOrderInfo(trackNumber);
            string checkpointAddress = await _geocoder.GetAddressByLongLat(info.Checkpoint.Location);
            string deliveryAddress = await _geocoder.GetAddressByLongLat(info.DeliveryPoint.Location);
            return new OrderInfoForClient(checkpointAddress, deliveryAddress, info.TrackNumber, info.Status);
        }

        public async Task<OrderTrackingInfo> GetTrackingInfo(string trackNumber)
        {
            var redisVal = _redis.StringGet(trackNumber);
            CustomerOrder? order;
            if (redisVal.HasValue && redisVal.IsInteger)
            {
                redisVal.TryParse(out int id);
                order = await _orderRepository.GetOrderInfo(id);
            }
            else
            {
                order = await _orderRepository.GetOrderInfo(trackNumber);
                if(order != null)
                    _redis.StringSet(order.TrackNumber, order.OrderId);
            }   
            if(order == null)
                throw new NotFoundException($"Failed to find order with track number: {trackNumber}");
            
            return await CreateTrackingInfo((OrderInfo)order);
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
            var tasks = orders.Select(o => CreateTrackingInfo((OrderInfo)o));
            return [.. (await Task.WhenAll(tasks))];
        }

        public async Task<List<CustomerOrder>> ListOrders()
        {
            return await _orderRepository.ListOrders();
        }

        public async Task<bool> UpdateOrder(OrderUpdateInfo info)
        {
            var report = await _orderRepository.UpdateOrderInfo(info);
            if (report == null)
                throw new NotFoundException($"Failed to find oder with track number: {info.TrackNumber}");
            if (report.Changed)
            {
               OrderTrackingInfo trackingInfo = await CreateTrackingInfo(report.OrderInfo);
               await SendOrderUpdatedEvent(trackingInfo);
            }
            return report.Changed;
        }
    }
}
