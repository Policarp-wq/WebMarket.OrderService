using NetTopologySuite.Geometries;
using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public interface ICheckpointRepository
    {
        Task<Checkpoint?> FindClosest(Point point);
        Task<Checkpoint> RegisterPoint(int userId, Point point, bool IsDeliveryPoint);
        Task<bool> DeletePoint(int pointId);

        Task<List<Checkpoint>> GetCheckpointsIdByOwner(int ownerId);
        Task<List<Checkpoint>> GetDeliveryPoints();
        Task<Checkpoint?> GetById(int id);
        Task<List<Checkpoint>> GetAll();
        
    }
}