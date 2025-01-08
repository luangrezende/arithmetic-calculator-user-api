﻿using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Application.Models.Response
{
    public class ApiResponse
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }
}
