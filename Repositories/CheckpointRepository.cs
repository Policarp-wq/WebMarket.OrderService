﻿using Microsoft.EntityFrameworkCore;
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
            if (!IsIdValid(pointId)) return false;
            var res = await _dbSet
                .Where(c => c.CheckpointId == pointId)
                .ExecuteDeleteAsync();
            return res > 0;
        }

        public async Task<Checkpoint?> FindClosest(Point point)
        {
            var pointParam = new NpgsqlParameter("pointParam", point);
            var res = await _dbSet.FromSql($"select * from fn_GetClosestPoint({pointParam})").FirstOrDefaultAsync();
            return res;
        }

        public async Task<List<Checkpoint>> GetAll()
        {
            var res =  await _dbSet
                .AsNoTracking()
                .ToListAsync();
            return res;
        }

        public async Task<Checkpoint?> GetById(int id)
        {
            if (id < 0) throw new InvalidArgumentException("Checkpoint id < 0: " + id);
            var res = await _dbSet
                .AsNoTracking()
                .Where(c => c.CheckpointId == id)
                .FirstOrDefaultAsync();
            return res;
        }

        public async Task<List<Checkpoint>> GetCheckpointsIdByOwner(int ownerId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<List<Checkpoint>> GetDeliveryPoints()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsDeliveryPoint).ToListAsync();
        }

        public async Task<Checkpoint> RegisterPoint(int userId, Point point, bool IsDeliveryPoint)
        {
            if (userId < 0) throw new InvalidArgumentException($"User id must be > 0!: {userId}");
            var res = await _dbSet.AddAsync(new Checkpoint()
            {
                OwnerId = userId,
                Location = point,
                IsDeliveryPoint = IsDeliveryPoint
                
            });
            await _context.SaveChangesAsync();
        
            return res.Entity;
        }

    }
}
