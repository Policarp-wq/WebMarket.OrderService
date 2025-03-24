using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMarket.OrderService.Models;

namespace OrderService.Tests
{
    public class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
    {
        protected readonly IServiceScope _scope;
        protected readonly OrdersDbContext _context;
        protected BaseIntegrationTest(IntegrationTestWebAppFactory factory) 
        {
            _scope = factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        }
    }
}
