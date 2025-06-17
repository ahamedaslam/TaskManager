using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Text.Json;
using TaskManager.Helper;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            _logger.LogDebug("Handling request: {Method} {Path}", context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
                _logger.LogDebug("Request handled successfully: {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GlobalException] Unhandled exception occurred.");
                _logger.LogWarning("Returning 500 Internal Server Error for request: {Method} {Path}", context.Request.Method, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var errorResponse = ResponseHelper.ServerError();
                var json = JsonSerializer.Serialize(errorResponse);

                await context.Response.WriteAsync(json);

                _logger.LogInformation("Error response sent to client.");
            }
        }
    }   

}

