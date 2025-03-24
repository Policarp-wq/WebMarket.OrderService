
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using WebMarket.OrderService.ApiContracts;
using WebMarket.OrderService.Models;
using WebMarket.OrderService.Services;
using WebMarket.OrderService.SupportTools.MapSupport;

namespace WebMarket.OrderService.AppExtensions.Endpoints
{
    public static class CheckpointEndpointExtension
    {
        public static IEndpointRouteBuilder AddCheckpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapPost("registerCheckpoint", RegisterCheckpoint);
            builder.MapGet("findClosestPoint", FindClosest);
            builder.MapGet("getAddressFromLongLat", GetAddress);
            builder.MapGet("getCheckpoints", GetAllCheckpoints);
            builder.MapGet("getOwnersPoints", GetOwnersCheckpoints);
            builder.MapGet("getDeliveryPoints", GetDeliveryCheckpoints);
            return builder;
        }

        public static async Task<Ok<List<CheckpointInfo>>> GetDeliveryCheckpoints(ICheckpointService checkpointService)
        {
            return TypedResults.Ok(await checkpointService.GetDeliveryCheckpoints());
        }

        public static async Task<Ok<List<CheckpointInfo>>> GetAllCheckpoints(ICheckpointService checkpointService)
        {
            return TypedResults.Ok(await checkpointService.GetAll());
        }

        public static async Task<Ok<List<CheckpointInfo>>> GetOwnersCheckpoints(ICheckpointService service, [FromQuery] int ownerId)
        {
            return TypedResults.Ok(await service.GetOwnersPoints(ownerId));
        }

        public static async Task<Results<Ok<string>, BadRequest<string>>> GetAddress(IMapGeocoder geocoder, ILogger<IMapGeocoder> logger, [FromQuery] double longitude, [FromQuery] double latitude)
        {
            try
            {
                string res = await geocoder.GetAddressByLongLat(longitude, latitude);
                return TypedResults.Ok(res);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "exception raised during call {method}", nameof(geocoder.GetAddressByLongLat));
                return TypedResults.BadRequest($"Failed to get address from {longitude}, {latitude}");
            }
        }

        private static async Task<Created<CheckpointInfo>> RegisterCheckpoint(
            ICheckpointService checkpointService,
            [FromQuery] int userId, [FromQuery] bool isDelivery, [FromBody] LocationPresentation checkPointLocation
            )
        {
            var res = await checkpointService.RegisterPoint(userId, (Point)checkPointLocation, isDelivery);
            return TypedResults.Created(nameof(RegisterCheckpoint), res);
        }

        private static async Task<Results<Ok<LocationPresentation>, NotFound<LocationPresentation>>> FindClosest(
            ICheckpointService checkpointService, [FromQuery] double x, [FromQuery] double y)
        {
            LocationPresentation checkPointLocation = new LocationPresentation(x, y);
            var checkpoint = await checkpointService.FindClosest((Point)checkPointLocation);
            if (checkpoint == null)
                return TypedResults.NotFound(checkPointLocation);
            return TypedResults.Ok((LocationPresentation)checkpoint.Location);
        }
        //AUTH!
        private static async Task<Results<Ok, NotFound>> Delete(
            ICheckpointService checkpointService,
            [FromQuery] int pointId)
        {
            var res = await checkpointService.DeletePoint(pointId);
            if (res)
                return TypedResults.Ok();
            else return TypedResults.NotFound();
        }
    }
}
