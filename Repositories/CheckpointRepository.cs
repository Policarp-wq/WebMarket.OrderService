using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NetTopologySuite.Geometries;
using Npgsql;
using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public class CheckpointRepository : BaseRepository<Checkpoint>, ICheckpointRepository
    {
        public CheckpointRepository(OrdersDbContext context) : base(context, context => context.Checkpoints)
        {
        }
        public async Task<bool> DeletePoint(int pointId)
        {
            if (pointId < 0) throw new InvalidArgumentException("Checkpoint id < 0: " + pointId);
            var res = await _dbSet
                .Where(c => c.CheckpointId == pointId)
                .ExecuteDeleteAsync();
            return res > 0;
        }

        public async Task<Checkpoint?> FindClosest(Point point)
        {
            var pointParam = new NpgsqlParameter("pointParam", point);
            var res = await _dbSet.FromSql($"select * from fn_GetClosestPoint({pointParam})").FirstOrDefaultAsync();
            if(res is null)
                return null;
            return res;
        }

        public async Task<List<Checkpoint>> GetAll()
        {
            var res =  await _dbSet
                .AsNoTracking()
                .ToListAsync();
            return res;
        }

        public async Task<Checkpoint> GetById(int id)
        {
            if (id < 0) throw new InvalidArgumentException("Checkpoint id < 0: " + id);
            var res = await _dbSet
                .AsNoTracking()
                .Where(c => c.CheckpointId == id)
                .FirstOrDefaultAsync();
            if (res == null)
                throw new NotFoundException("Failed to find checkpoint with id: " + id);
            return res;
        }

        public async Task<List<Checkpoint>> GetCheckpointsIdByOwner(int ownerId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.OwnerId == ownerId)
                .ToListAsync()
                ;
        }

        public async Task<Checkpoint> RegisterPoint(int userId, Point point)
        {
            if (userId < 0) throw new InvalidArgumentException("User id < 0: " + userId);
            var res = await _dbSet.AddAsync(new Checkpoint()
            {
                OwnerId = userId,
                Location = point
            });
            await _context.SaveChangesAsync();
        
            return res.Entity;
        }

    }
}
