using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebMarket.OrderService.Exceptions
{
    public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            
            httpContext.Response.StatusCode = exception switch
            {
                PrivateServerException => StatusCodes.Status500InternalServerError,
                ServerException => StatusCodes.Status400BadRequest,
                HttpRequestException => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };
            if (exception is PrivateServerException)
                return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext() { HttpContext = httpContext, Exception = new Exception("Inner exception") });
            //logger.LogError("Exception caught: " + exception.GetType().FullName + " " + exception.Message);
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext()
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails()
                {
                    Type = exception.GetType().Name,
                    Title = "An error occured",
                    Detail = exception.Message,
                }
            });
        }
    }
}
