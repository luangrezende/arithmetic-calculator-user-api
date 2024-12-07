using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Application.Models.Response
{
    public class TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("expiration")]
        public DateTime Expiration { get; set; }
    }
}
