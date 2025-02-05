using NetTopologySuite.Geometries;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Services
{
    public interface ICheckpointService
    {
        Task<Checkpoint?> FindClosest(Point point);
        Task<List<Checkpoint>> GetUsersPoints(int userId);
        Task<Checkpoint> RegisterPoint(int userId, Point point);
        Task<bool> DeletePoint(int pointId);
    }
}