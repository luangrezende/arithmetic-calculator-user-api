using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Domain.Models.Request
{
    public class AddBalanceRequest
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}
