using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace StochasticBackend.src.Shared.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"An unhandled exception occurred. {exception.Message}");

            var (statusCode, title) = exception switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            // Standardized RFC 7807 problem details object
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
