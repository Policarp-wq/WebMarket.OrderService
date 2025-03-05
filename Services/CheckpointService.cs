using NetTopologySuite.Geometries;
using StackExchange.Redis;
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
            if (pointId < 0) throw new ArgumentOutOfRangeException();
            return await _checkpointRepository.DeletePoint(pointId);
        }

        public async Task<Checkpoint?> FindClosest(Point point)
        {
            return await _checkpointRepository.FindClosest(point);
        }

        public async Task<List<Checkpoint>> GetAll()
        {
            return await _checkpointRepository.GetAll();
        }

        public async Task<List<Checkpoint>> GetOwnersPoints(int ownerId)
        {
            return await _checkpointRepository.GetCheckpointsIdByOwner(ownerId);
        }

        public async Task<Checkpoint> RegisterPoint(int userId, Point point)
        {
            return await _checkpointRepository.RegisterPoint(userId, point);
        }
    }
}
