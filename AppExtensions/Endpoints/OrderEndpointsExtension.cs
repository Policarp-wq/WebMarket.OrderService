using Confluent.Kafka;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebMarket.OrderService.ApiContracts;
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
            builder.MapPatch("updateOrderStatus", UpdateOrderStatus);
            builder.MapPatch("updateOrderCheckpoint", UpdateOrderCheckpoint);
            builder.MapGet("getOrders", GetOrders); 
            builder.MapGet("getOrderStatuses", GetPossibleStatuses);
            builder.MapGet("getOrderByTrackNumber", GetOrderByTrackNumber);
            builder.MapGet("getUsersOrders", GetUsersOrders);
            return builder;
        }

        private static async Task <Ok<OrderTrackingInfo>> GetOrderByTrackNumber(IOrderService orderService, [FromQuery] string trackNumber)
        {
            return TypedResults.Ok(await orderService.GetTrackingInfo(trackNumber));
        }

        private static async Task<Ok<List<OrderTrackingInfo>>> GetUsersOrders(IOrderService orderService, [FromQuery] int userId)
        {
            return TypedResults.Ok(await orderService.GetUsersOrders(userId));
        }

        private static async Task<Ok<List<CustomerOrder>>> GetOrders(IOrderService orderService)
        {
            return TypedResults.Ok(await orderService.ListOrders());
        }

        private static Ok<string[]> GetPossibleStatuses()
        {
            return TypedResults.Ok(Enum.GetNames(typeof(CustomerOrder.OrderStatus)));
        }

        public static async Task<Ok<string>> CreateOrder(IOrderService orderService, OrderCreateInfo createInfo)
        {
            var trackNumber = await orderService.CreateOrder(createInfo.CustomerID, createInfo.ProductID,
                createInfo.DeliveryPointID, createInfo.ProductOwnerId);
            return TypedResults.Ok(trackNumber);

        }

        public static async Task<Ok<bool>> UpdateOrder(IOrderService orderService, OrderUpdateInfo orderInfo)
        {
            var updated = await orderService.UpdateOrder(orderInfo);
            return TypedResults.Ok(updated);
        }

        public static async Task<Ok<bool>> UpdateOrderStatus(IOrderService orderService, [FromQuery] string trackNumber, CustomerOrder.OrderStatus orderStatus)
        {
            return await UpdateOrder(orderService, new OrderUpdateInfo(trackNumber, null, orderStatus));
        }

        public static async Task<Ok<bool>> UpdateOrderCheckpoint(IOrderService orderService, [FromQuery] string trackNumber, int checkpointId)
        {
            return await UpdateOrder(orderService, new OrderUpdateInfo(trackNumber, checkpointId, null));
        }
    }
}
