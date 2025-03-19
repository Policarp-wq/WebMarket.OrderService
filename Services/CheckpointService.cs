using NetTopologySuite.Geometries;
using StackExchange.Redis;
using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;

namespace WebMarket.OrderService.Services
{
    public class CheckpointService : BaseService, ICheckpointService
    {
        private ICheckpointRepository _checkpointRepository;
        private IDatabase _redis;
        public CheckpointService(ICheckpointRepository repository, IConnectionMultiplexer multiplexer)
        {
            _checkpointRepository = repository;
            _redis = multiplexer.GetDatabase();       
        }

        public async Task<bool> DeletePoint(int pointId)
        {
            return await _checkpointRepository.DeletePoint(pointId);
        }

        public async Task<CheckpointInfo?> FindClosest(Point point)
        {
            var res = await _checkpointRepository.FindClosest(point);
            if(res is null)
                return null;
            return res;
        }

        public async Task<List<CheckpointInfo>> GetAll()
        {
            var l = await _checkpointRepository.GetAll();
            return l.Select(c => (CheckpointInfo)c).ToList();
        }

        public async Task<List<CheckpointInfo>> GetDeliveryCheckpoints()
        {
            return (await _checkpointRepository.GetDeliveryPoints())
                .Select(c => (CheckpointInfo)c)
                .ToList();
        }

        public async Task<List<CheckpointInfo>> GetOwnersPoints(int ownerId)
        {
            var l = await _checkpointRepository.GetCheckpointsIdByOwner(ownerId);
            return l.Select(c => (CheckpointInfo)c).ToList();
        }

        public async Task<CheckpointInfo> RegisterPoint(int userId, Point point)
        {
            return await _checkpointRepository.RegisterPoint(userId, point);
        }
    }
}
