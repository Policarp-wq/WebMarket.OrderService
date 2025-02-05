using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.AppExtensions.Endpoints
{
    public static class OrderEndpointsExtension
    {
        public static IEndpointRouteBuilder AddOrderEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapGet("index", Index);
            return builder;
        }
        public static async Task<IResult> Index(OrdersDbContext context)
        {
            throw new NotImplementedException();
        }
    }
}
