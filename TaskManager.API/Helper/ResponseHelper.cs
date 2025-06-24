using TaskManager.Models.Response;
using TaskManager.Models.Responses;

namespace TaskManager.Helper
{
    public static class ResponseHelper
    {
       // Non-generic success
        public static Response Success(string?logId = null, object? data = null, string message = "Request completed successfully.")
        {
            var fullMessage = logId != null ? $"Log ID: [{logId}] {message}" : message;
            return new Response
            {
                ResponseCode = 0,
                ResponseDescription = message,
                ResponseDatas = fullMessage
            };
        }

        //  Generic success
        public static Response<T> SuccessGeneric<T>(T data, string? message = null)
        {
            return new Response<T>
            {
                ResponseCode = 0,
                ResponseDescription = message ?? "Request completed successfully.",
                ResponseDatas = data
            };
        }

        ////  Non-generic bad request
        public static Response BadRequest(string?logId = null, string message = "Oops! Something seems off with your request. Please check and try again.")
        {
            var fullMessage = logId != null ? $"Log ID: [{logId}] {message}" : message;
           
            return new Response
            {
                ResponseCode = 1001,
                ResponseDescription = fullMessage
            };
        }

        // Generic bad request
        //public static Response<T> BadRequest<T>(string message = "Oops! Something seems off with your request. Please check and try again.")
        //{
        //    return new Response<T>
        //    {
        //        ResponseCode = 1001,
        //        ResponseDescription = message
        //    };
        //}

        public static Response Unauthorized(string?logId = null, string message = "Access denied..!!")
        {
            var fullMessage = logId != null ? $"Log ID: [{logId}] {message}" : message;
            return new Response
            {
                ResponseCode = 1002,
                ResponseDescription = fullMessage
            };
        }

        //public static Response<T> Unauthorized<T>(string message = "Access denied..!!")
        //{
        //    return new Response<T>
        //    {
        //        ResponseCode = 1002,
        //        ResponseDescription = message
        //    };
        //}

        //It creates a string called fullMessage that either includes a log ID (if provided) or just the message.
        public static Response NotFound(string?logId = null, string message = "We couldn’t find what you’re looking for.")
        {// shorthand for an if-else statement.
            var fullMessage = logId != null ? $"Log ID: [{logId}] {message}" : message; //ternary operator to check if logId is not null
            return new Response
            {
                ResponseCode = 1003,
                ResponseDescription = fullMessage
            };
        }

        public static Response<T> NotFound<T>(string message = "We couldn’t find what you’re looking for.")
        {
            return new Response<T>
            {
                ResponseCode = 1003,
                ResponseDescription = message,
                ResponseDatas = default
            };
        }

        public static Response Conflict(string message = "A conflict occurred with your request.")
        {
            return new Response
            {
                ResponseCode = 1004,
                ResponseDescription = message
            };
        }

        //public static Response<T> Conflict<T>(string message = "A conflict occurred with your request.")
        //{
        //    return new Response<T>
        //    {
        //        ResponseCode = 1004,
        //        ResponseDescription = message
        //    };
        //}

        public static Response Unauthenticated(string message = "Authentication required. Please log in.")
        {
            return new Response
            {
                ResponseCode = 1007,
                ResponseDescription = message
            };
        }

        //public static Response<T> Unauthenticated<T>(string message = "Authentication required. Please log in.")
        //{
        //    return new Response<T>
        //    {
        //        ResponseCode = 1007,
        //        ResponseDescription = message
        //    };
        //}

        public static Response Unprocessable(string message = "Unprocessable request.")
        {
            return new Response
            {
                ResponseCode = 1005,
                ResponseDescription = message
            };
        }

        //public static Response<T> Unprocessable<T>(string message = "Unprocessable request.")
        //{
        //    return new Response<T>
        //    {
        //        ResponseCode = 1005,
        //        ResponseDescription = message
        //    };
        //}

        public static Response ServerError(string? logId = null, string message = "Something went wrong on our end. Please try again later.")
        {
            //It creates a string called fullMessage that either includes a log ID (if provided) or just the message

            var fullMessage = logId != null ? $"Log ID: [{logId}] {message}" : message;

            return new Response
            {
                ResponseCode = 1006,
                ResponseDescription = fullMessage
            };
        }

        public static Response<T> ServerError<T>(string message = "Something went wrong on our end. Please try again later.")
        {
            return new Response<T>
            {
                ResponseCode = 1006,
                ResponseDescription = message
            };
        }
    }
}
