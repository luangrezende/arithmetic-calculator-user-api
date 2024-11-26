using ArithmeticCalculatorUserApi.Domain.Enums;
using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Domain.Models
{
    public class BankAccount
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("accountType")]
        public AccountType AccountType { get; set; }

        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }
    }
}
