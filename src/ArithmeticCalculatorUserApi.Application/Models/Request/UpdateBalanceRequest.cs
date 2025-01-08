using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Application.Models.Request
{
    public class UpdateBalanceRequest
    {
        [Required(ErrorMessage = "accountId is required.")]
        [JsonPropertyName("accountId")]
        public Guid AccountId { get; set; }

        [Required(ErrorMessage = "amount is required.")]
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}
