using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Domain.Models
{
    public class AuthenticateUser
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
