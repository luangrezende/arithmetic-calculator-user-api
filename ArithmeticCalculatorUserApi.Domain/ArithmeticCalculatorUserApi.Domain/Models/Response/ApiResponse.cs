namespace ArithmeticCalculatorUserApi.Domain.Models.Response
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }

        public object? Data { get; set; }
    }
}
