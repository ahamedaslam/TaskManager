namespace TaskManager.Helper
{
    public static class HttpStatusMapper  //cant able to create object of this class, so making it static
    {
        public static int GetHttpStatusCode(int responseCode)
        {
            return responseCode switch
            {
                0 => 200,       // Success
                1001 => 400,    // Invalid request
                1002 => 403,    // Access denied
                1003 => 404,    // Resource not found
                1004 => 409,    // Conflict
                1005 => 422,    // Unprocessable
                1006 => 500,    // Unknown or internal error
                _ => 500        // Default to Internal Server Error for unknown codes


            };
            
        }
    }
}
