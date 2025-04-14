using Confluent.Kafka;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Repositories;
using WebMarket.OrderService.Services;
using WebMarket.OrderService.SupportTools.Kafka;


namespace WebMarket.OrderService.AppExtensions.Endpoints
{
    public static class OrderEndpointsExtension
    {
        public static IEndpointRouteBuilder AddOrderEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapPost("createOrder", CreateOrder);
            builder.MapPatch("updateOrder", UpdateOrder);
            builder.MapGet("getOrders", GetOrders); 
            builder.MapGet("getOrderStatuses", GetPossibleStatuses);
            builder.MapGet("getOrderByTrackNumber", GetOrderByTrackNumber);
            builder.MapGet("getUsersOrders", GetUsersOrders);
            return builder;
        }

        private static readonly string[] OrderStatuses = Enum.GetNames(typeof(OrderStatus));

        private static async Task <Ok<OrderInfoForCustomer>> GetOrderByTrackNumber(IOrderService orderService, [FromQuery] string trackNumber)
        {
            return TypedResults.Ok(await orderService.GetOrderInfo(trackNumber));
        }

        private static async Task<Ok<List<OrderInfoForCustomer>>> GetUsersOrders(IOrderService orderService, [FromQuery] int userId)
        {
            return TypedResults.Ok(await orderService.GetUsersOrders(userId));
        }

        private static async Task<Ok<List<CustomerOrder>>> GetOrders(IOrderService orderService)
        {
            return TypedResults.Ok(await orderService.ListOrders());
        }

        private static Ok<string[]> GetPossibleStatuses()
        {
            return TypedResults.Ok(OrderStatuses);
        }

        public static async Task<Ok<string>> CreateOrder(IOrderService orderService, OrderCreateInfo createInfo)
        {
            var trackNumber = await orderService.CreateOrder(createInfo.CustomerID, createInfo.ProductID,
                createInfo.DeliveryPointID, createInfo.ProductOwnerId);
            return TypedResults.Ok(trackNumber);

        }

        public static async Task<Results<Ok<bool>, BadRequest<string>>> UpdateOrder(IOrderService orderService, [FromQuery] string trackNumber, [FromQuery] string status)
        {
            if (Enum.TryParse(status, out OrderStatus orderStatus))
            {
                var updated = await orderService.UpdateOrder(trackNumber, orderStatus);
                return TypedResults.Ok(updated);
            }
            return TypedResults.BadRequest($"Wrong order status: {status}");
        }

    }
}
