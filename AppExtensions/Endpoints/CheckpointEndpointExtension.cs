
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
            builder.MapGet("getClosestPoint", FindClosest);
            builder.MapGet("getAdd", GetAddress);
            builder.MapGet("getCheckpoints", GetAllCheckpoints);
            return builder;
        }

        public static async Task<IResult> GetAllCheckpoints(ICheckpointService checkpointService)
        {
            return Results.Ok(await checkpointService.GetAll());
        }

        public static async Task<IResult> GetAddress(IMapGeocoder geocoder, ILogger<IMapGeocoder> logger, [FromQuery] double longitude, [FromQuery] double latitude)
        {
            try
            {
                string res = await geocoder.GetAddressByLongLat(longitude, latitude);
                return Results.Ok(res);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "exception raised during call {method}", nameof(geocoder.GetAddressByLongLat));
                return Results.Ok($"{longitude}, {latitude}");
            }
        }

        private static async Task<IResult> RegisterCheckpoint(
            ICheckpointService checkpointService,
            [FromQuery] int userId, [FromBody] LocationPresentation checkPointLocation
            )
        {
            var res = await checkpointService.RegisterPoint(userId, (Point)checkPointLocation);
            return Results.Created(nameof(RegisterCheckpoint), (LocationPresentation)res.Location);
        }

        private static async Task<IResult> FindClosest(
            ICheckpointService checkpointService, [FromQuery] double x, [FromQuery] double y)
        {
            LocationPresentation checkPointLocation = new LocationPresentation(x, y);
            var checkpoint = await checkpointService.FindClosest((Point)checkPointLocation);
            if (checkpoint == null)
                return Results.NotFound(checkPointLocation);
            return Results.Ok((LocationPresentation)checkpoint.Location);
        }
        //AUTH!
        private static async Task<IResult> Delete(
            ICheckpointService checkpointService,
            [FromQuery] int pointId)
        {
            var res = await checkpointService.DeletePoint(pointId);
            if (res)
                return Results.Ok();
            else return Results.NotFound();
        }
    }
}
