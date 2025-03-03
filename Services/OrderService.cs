using Newtonsoft.Json;
using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.SupportTools.Kafka;
using WebMarket.OrderService.SupportTools.TrackNumber;

namespace WebMarket.OrderService.Services
{
    public class OrderService : BaseService, IOrderService
    {
        private IOrderRepository _orderRepository;
        private ICheckpointRepository _checkpointRepository;
        private ITrackNumberGenerator _trackNumberGenerator;
        private IKafkaMessageProducer _producer;
        public OrderService(IOrderRepository orderRepository,  ITrackNumberGenerator trackNumberGenerator, ICheckpointRepository checkpointRepository, IKafkaMessageProducer messageProducer)
        {
            _orderRepository = orderRepository;
            _trackNumberGenerator = trackNumberGenerator;
            _checkpointRepository = checkpointRepository;
            _producer = messageProducer;
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

        public async Task<List<CustomerOrder>> ListOrders()
        {
            return await _orderRepository.ListOrders();
        }

        public async Task<bool> UpdateOrder(UpdateOrderInfo info)
        {
            var report = await _orderRepository.UpdateOrderInfo(info);
            if (report.Changed)
            {
                _producer.ProduceMessage("order_update", info.OrderID.ToString(), JsonConvert.SerializeObject(report.OrderInfo));
            }
            return report.Changed;
        }
    }
}
