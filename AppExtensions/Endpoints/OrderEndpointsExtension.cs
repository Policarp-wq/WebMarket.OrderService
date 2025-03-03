using Confluent.Kafka;
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
            return builder;
        }

        private static async Task<IResult> GetOrders(IOrderService orderService)
        {
            return Results.Ok(await orderService.ListOrders());
        }

        public static async Task<IResult> CreateOrder(IOrderService orderService, OrderCreateInfo createInfo)
        {
            var trackNumber = await orderService.CreateOrder(createInfo.CustomerID, createInfo.ProductID, createInfo.DeliveryPointID, createInfo.SupplierId);
            return Results.Ok(trackNumber);

        }

        public static async Task<IResult> UpdateOrder(IOrderService orderService, UpdateOrderInfo orderInfo)
        {
            var updated = await orderService.UpdateOrder(orderInfo);
            return Results.Ok(updated);
        }

        public static async Task<IResult> UpdateOrderStatus(IOrderService orderService, [FromQuery] int orderId, CustomerOrder.OrderStatus orderStatus)
        {
            return await UpdateOrder(orderService, new UpdateOrderInfo(orderId, null, orderStatus));
        }

        public static async Task<IResult> UpdateOrderCheckpoint(IOrderService orderService, [FromQuery] int orderId, int checkpointId)
        {
            return await UpdateOrder(orderService, new UpdateOrderInfo(orderId, checkpointId, null));
        }
    }
}
