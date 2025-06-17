using TaskManager.Models;

namespace TaskManager.Helper
{
    public static class ResponseHelper
    {
        public static Response Success(object? data = null, string message = "Request completed successfully.")
        {
            return new Response
            {
                ResponseCode = 0,
                ResponseDescription = message,
                ResponseDatas = data
            };
        }

        public static Response BadRequest(string message = "Oops! Something seems off with your request. Please check and try again.")
        {
            return new Response
            {

                ResponseCode = 1001, // 400
                ResponseDescription = message
            };
        }

        public static Response Unauthorized(string message = "You're not authorized to perform this action.")
        {
            return new Response
            {
                ResponseCode = 1002, // 403
                ResponseDescription = message
            };
        }

        public static Response NotFound(string message = "We couldn’t find what you’re looking for.")
        {
            return new Response
            {
                ResponseCode = 1003, // ✅ Corrected from 1002 to 1003 (404)
                ResponseDescription = message
            };
        }

        public static Response Conflict(string message = "A conflict occurred with your request.")
        {
            return new Response
            {
                ResponseCode = 1004, // 409
                ResponseDescription = message
            };
        }

        public static Response Unprocessable(string message = "Unprocessable request.")
        {
            return new Response
            {
                ResponseCode = 1005, // 422
                ResponseDescription = message
            };
        }

        public static Response ServerError(string message = "Something went wrong on our end. Please try again later.")
        {
            return new Response
            {
                ResponseCode = 1006, // Not using custom? Use 1006 if sticking to pattern
                ResponseDescription = message
            };
        }
    }
}
