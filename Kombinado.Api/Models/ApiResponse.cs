namespace Kombinado.Api.Models
{
    public class ApiResponse<T> where T : class
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; }

        public static ApiResponse<T> SuccessResponse(string message, T? data = null, int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> FailureResponse(string message, int statusCode = 500)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = null,
                StatusCode = statusCode
            };
        }
    }
}
