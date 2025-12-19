using System.Security.Authentication;
using System.Text.Json;
using TaskManager.Helper;
using TaskManager.Models.Response;

//Centralized logging
namespace TaskManager.Middleware
{

    //to handle unexpected or unhandled errors globally.


    //ExceptionHandleMiddleware is a custom middleware for ASP.NET Core that catches any unhandled exceptions during HTTP request processing.
    //Instead of letting the app crash or return a generic error, it logs the error and returns a standardized JSON error response to the client.

    //•	For every HTTP request, this middleware runs.
    //•	If any unhandled exception occurs in your controllers or other middleware, it:
    //•	Logs the error.
    //•	Returns a JSON error message with HTTP 500 status.

    //Why use this?
    //•	Centralized error handling: No need to wrap every controller in try/catch.
    //•	Consistent error responses: Clients always get a predictable JSON error.
    //•	Better logging: All unhandled exceptions are logged for diagnostics.




    public class ExceptionHandleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandleMiddleware> _logger;

        public ExceptionHandleMiddleware(RequestDelegate next, ILogger<ExceptionHandleMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _logger.LogInformation("ExceptionHandleMiddleware initialized.");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("ExceptionHandleMiddleware initialized.");
            _logger.LogDebug("Handling request: {Method} {Path}", context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
                _logger.LogDebug("Request handled successfully: {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GlobalException] Unhandled exception occurred");

                context.Response.ContentType = "application/json";

                (int statusCode, Response response) = ex switch
                {
                    ArgumentException => (
                        StatusCodes.Status400BadRequest,
                        ResponseHelper.BadRequest(ex.Message) //Bad request
                    ),

                    UnauthorizedAccessException => (
                        StatusCodes.Status403Forbidden, //un-authorized
                        ResponseHelper.Unauthorized()
                    ),

                    AuthenticationException => (
                        StatusCodes.Status401Unauthorized, //un-authenticated
                        ResponseHelper.Unauthenticated()
                    ),

                    KeyNotFoundException => (
                        StatusCodes.Status404NotFound,
                        ResponseHelper.NotFound(ex.Message) //resource-not found
                    ),

                    InvalidOperationException => (
                        StatusCodes.Status409Conflict,
                        ResponseHelper.Conflict(ex.Message)
                    ),

                    _ => (
                        StatusCodes.Status500InternalServerError,
                        ResponseHelper.ServerError()
                    )
                };

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }

        }
    }   

}

