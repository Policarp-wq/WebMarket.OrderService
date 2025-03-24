using NetTopologySuite.Geometries;
using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Services
{
    public interface ICheckpointService
    {
        Task<CheckpointInfo?> FindClosest(Point point);
        Task<List<CheckpointInfo>> GetOwnersPoints(int ownerId);
        Task<CheckpointInfo> RegisterPoint(int userId, Point point, bool IsDeliveryPoint);
        Task<List<CheckpointInfo>> GetDeliveryCheckpoints();
        Task<bool> DeletePoint(int pointId);
        Task<List<CheckpointInfo>> GetAll();
    }
}