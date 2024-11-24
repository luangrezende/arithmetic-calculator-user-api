using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Domain.Models.Request
{
    public class TokenRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
