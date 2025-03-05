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
            builder.MapGet("getOrderStatuses", GetPossibleStatuses);
            builder.MapGet("getOrderByTrackNumber", GetOrderByTrackNumber);
            return builder;
        }

        private static async Task <IResult> GetOrderByTrackNumber(IOrderService orderService, [FromQuery] string trackNumber)
        {
            return Results.Ok(await orderService.GetOrderInfo(trackNumber));
        }

        private static async Task<IResult> GetOrders(IOrderService orderService)
        {
            return Results.Ok(await orderService.ListOrders());
        }

        private static IResult GetPossibleStatuses()
        {
            return Results.Ok(Enum.GetNames(typeof(CustomerOrder.OrderStatus)));
        }

        public static async Task<IResult> CreateOrder(IOrderService orderService, OrderCreateInfo createInfo)
        {
            var trackNumber = await orderService.CreateOrder(createInfo.CustomerID, createInfo.ProductID, createInfo.DeliveryPointID, createInfo.SupplierId);
            return Results.Ok(trackNumber);

        }

        public static async Task<IResult> UpdateOrder(IOrderService orderService, OrderUpdateInfo orderInfo)
        {
            var updated = await orderService.UpdateOrder(orderInfo);
            return Results.Ok(updated);
        }

        public static async Task<IResult> UpdateOrderStatus(IOrderService orderService, [FromQuery] string trackNumber, CustomerOrder.OrderStatus orderStatus)
        {
            return await UpdateOrder(orderService, new OrderUpdateInfo(trackNumber, null, orderStatus));
        }

        public static async Task<IResult> UpdateOrderCheckpoint(IOrderService orderService, [FromQuery] string trackNumber, int checkpointId)
        {
            return await UpdateOrder(orderService, new OrderUpdateInfo(trackNumber, checkpointId, null));
        }
    }
}
