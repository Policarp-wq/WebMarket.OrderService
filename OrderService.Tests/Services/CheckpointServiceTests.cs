using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMarket.OrderService.Services;

namespace OrderService.Tests.Services
{
    public class CheckpointServiceTests : BaseIntegrationTest, IAsyncLifetime
    {
        private readonly ICheckpointService _checkpointService;
        
        public CheckpointServiceTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
            _checkpointService = _scope.ServiceProvider.GetRequiredService<ICheckpointService>();
        }
        

        public async Task DisposeAsync()
        {
            await _respawnDb();
            await _respawnRedis();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
