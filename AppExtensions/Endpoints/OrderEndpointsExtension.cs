using Confluent.Kafka;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebMarket.OrderService.DTO.Order;
using WebMarket.OrderService.DTO.OrderStatusStory;
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
            builder.MapGet("getOrders", GetOrders); 
            builder.MapGet("getOrderStatuses", GetPossibleStatuses);
            builder.MapGet("getOrderByTrackNumber", GetOrderByTrackNumber);
            builder.MapGet("getUsersOrders", GetUsersOrders);
            builder.MapGet("deliverOrderToCheckpoint", DeliverOrderToCheckpoint);
            builder.MapGet("sentToNext", SentToNext);
            builder.MapGet("getOrderRoute", GetOrderRoute);
            builder.MapGet("getOrderStatusStory", GetOrderStatusStory);

            return builder;
        }

        private static readonly string[] OrderStatuses = Enum.GetNames(typeof(OrderStatus));

        private static async Task <Ok<OrderInfoForCustomer>> GetOrderByTrackNumber(IOrderService orderService, [FromQuery] string trackNumber)
        {
            return TypedResults.Ok(await orderService.GetOrderInfo(trackNumber));
        }
        private static async Task<Ok<bool>> DeliverOrderToCheckpoint(IOrderTraceService traceService, [FromQuery] string trackNumber)
        {
            return TypedResults.Ok(await traceService.SetOrderDelivered(trackNumber, DateTime.UtcNow));
        }
        private static async Task<Ok<bool>> SentToNext(IOrderTraceService traceService, [FromQuery] string trackNumber, int checkpointId)
        {
            return TypedResults.Ok(await traceService.AddRouteUnit(trackNumber, checkpointId));
        }

        private static async Task<Ok<OrderTraceRoute>> GetOrderRoute(IOrderTraceService traceService, [FromQuery]string trackNumber)
        {
            return TypedResults.Ok(await traceService.GetOrderRoute(trackNumber));
        }
        private static async Task<Ok<OrderStatusStoryForClient>> GetOrderStatusStory(IOrderStatusStoryService storyService, [FromQuery]string trackNumber)
        {
            return TypedResults.Ok(await storyService.GetOrderStatusStory(trackNumber));
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

    }
}
