using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
        {
            // 1. Log the exception with its details
            _logger.LogError(
                exception,
                "An unhandled exception occurred. TraceId: {TraceId}",
                httpContext.TraceIdentifier);

            // 2. Set the HTTP status code
            httpContext.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            // 3. Return a safe response to the client
            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    traceId = httpContext.TraceIdentifier
                },
                cancellationToken);

            // 4. Tell ASP.NET Core that the exception was handled
            return true;
        }
    }
}
