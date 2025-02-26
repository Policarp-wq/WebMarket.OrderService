﻿using NetTopologySuite.Geometries;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;

namespace WebMarket.OrderService.Services
{
    public class CheckpointService : BaseService, ICheckpointService
    {
        private ICheckpointRepository _checkpointRepository;
        public CheckpointService(ICheckpointRepository repository)
        {
            _checkpointRepository = repository;
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

        public async Task<List<Checkpoint>> GetUsersPoints(int userId)
        {
            return await _checkpointRepository.GetUsersPoints(userId);
        }

        public async Task<Checkpoint> RegisterPoint(int userId, Point point)
        {
            return await _checkpointRepository.RegisterPoint(userId, point);
        }
    }
}
