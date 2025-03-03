using NetTopologySuite.Geometries;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public interface ICheckpointRepository
    {
        Task<Checkpoint?> FindClosest(Point point);
        Task<List<Checkpoint>> GetUsersPoints(int userId);
        Task<Checkpoint> RegisterPoint(int userId, Point point);
        Task<bool> DeletePoint(int pointId);

        Task<List<Checkpoint>> GetCheckpointsIdByOwner(int ownerId);
        Task<Checkpoint> GetById(int id);
        
    }
}