using ArithmeticCalculatorUserApi.Domain.Models.Response;
using System.Net;

namespace ArithmeticCalculatorUserApi.Helpers
{
    public static class ApiResponseHelper
    {
        public static ApiResponse CreateErrorResponse(HttpStatusCode statusCode, string errorMessage)
        {
            return new ApiResponse
            {
                StatusCode = (int)statusCode,
                Data = new { error = errorMessage }
            };
        }
    }
}
