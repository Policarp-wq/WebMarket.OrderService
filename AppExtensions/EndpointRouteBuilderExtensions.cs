using WebMarket.OrderService.AppExtensions.Endpoints;

namespace WebMarket.OrderService.AppExtensions
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder AddEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            var group = routeBuilder.MapGroup("/api");
            group.AddOrderEndpoints();
            group.AddCheckpoints();
            return routeBuilder;
        }
    }
}
