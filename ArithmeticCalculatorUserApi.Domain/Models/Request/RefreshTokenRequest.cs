﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Domain.Models.Request
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "refreshToken is required.")]
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
