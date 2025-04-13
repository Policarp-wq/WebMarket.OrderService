using NetTopologySuite.Geometries;
using StackExchange.Redis;
using WebMarket.OrderService.DTO.Checkpoints;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.SupportTools.MapSupport;

namespace WebMarket.OrderService.Services
{
    public class CheckpointService : BaseService, ICheckpointService
    {
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly IDatabase _redis;
        private readonly IMapGeocoder _geocoder;
        public CheckpointService(ICheckpointRepository repository, IConnectionMultiplexer multiplexer, IMapGeocoder geocoder)
        {
            _checkpointRepository = repository;
            _redis = multiplexer.GetDatabase();
            _geocoder = geocoder;
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
        //TODO: optimize
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

        public async Task<CheckpointInfo> RegisterPoint(int userId, Point point, bool IsDeliveryPoint)
        {
            string? address = await _geocoder.GetAddressByLongLat(point);
            return await _checkpointRepository.RegisterPoint(userId, point, IsDeliveryPoint, address);
        }
    }
}
