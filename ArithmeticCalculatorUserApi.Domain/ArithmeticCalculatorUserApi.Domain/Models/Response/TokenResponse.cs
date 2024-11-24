using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Domain.Models.Response
{
    public class TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("validation")]
        public int Validation { get; set; }
    }
}
