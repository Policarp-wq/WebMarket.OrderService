using Newtonsoft.Json;
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
        private IOrderRepository _orderRepository;
        private ICheckpointRepository _checkpointRepository;
        private ITrackNumberGenerator _trackNumberGenerator;
        private IKafkaMessageProducer _producer;
        private IMapGeocoder _geocoder;
        public OrderService(IOrderRepository orderRepository,  ITrackNumberGenerator trackNumberGenerator,
            ICheckpointRepository checkpointRepository, IKafkaMessageProducer messageProducer,
            IMapGeocoder geocoder)
        {
            _orderRepository = orderRepository;
            _trackNumberGenerator = trackNumberGenerator;
            _checkpointRepository = checkpointRepository;
            _producer = messageProducer;
            _geocoder = geocoder;
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

        public async Task<string> CreateOrder(int customerID, int productID, int deliverypointID, int supplierId)
        {
            var deliveryCheckpointTask = await _checkpointRepository.GetById(deliverypointID);
            var suppliersCheckpointsTask = await _checkpointRepository.GetCheckpointsIdByOwner(supplierId);
            if (!suppliersCheckpointsTask.Any())
                throw new NotFoundException("No checkpoints for given supplier " + supplierId);
            var closestSupplier = FindClosest(deliveryCheckpointTask, suppliersCheckpointsTask);

            var trackNum = _trackNumberGenerator.GenerateTrackNumber();
            var createdOrder = await _orderRepository.CreateOrder(customerID, productID, deliverypointID, closestSupplier.CheckpointId, trackNum);
            //add redis
            if (createdOrder is null)
                throw new PrivateServerException($"Created null order ?? cust: {customerID}, prod: {productID}, deliv: {deliverypointID}, track: {trackNum}");
            return createdOrder.TrackNumber;
        }


        public async Task<OrderInfo> GetOrderInfo(string trackNumber)
        {
            var info = await _orderRepository.GetOrderInfo(trackNumber);
            return info;
        }

        public async Task<OrderInfoForClient> GetOrderInfoForClient(string trackNumber)
        {
            var info = await GetOrderInfo(trackNumber);
            string checkpointAddress = await _geocoder.GetAddressByLongLat(info.Checkpoint.Location);
            string deliveryAddress = await _geocoder.GetAddressByLongLat(info.DeliveryPoint.Location);
            return new OrderInfoForClient(checkpointAddress, deliveryAddress, info.TrackNumber, info.Status);
        }

        public async Task<List<CustomerOrder>> ListOrders()
        {
            return await _orderRepository.ListOrders();
        }

        public async Task<bool> UpdateOrder(UpdateOrderInfo info)
        {
            var report = await _orderRepository.UpdateOrderInfo(info);
            if (report.Changed)
            {
               await _producer.ProduceMessage("order_update", info.OrderID.ToString(), JsonConvert.SerializeObject(report.OrderInfo));
            }
            return report.Changed;
        }
    }
}
