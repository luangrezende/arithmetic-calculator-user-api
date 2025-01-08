﻿using Amazon.Lambda.APIGatewayEvents;
using ArithmeticCalculatorUserApi.Application.Models.Response;
using System.Net;
using System.Text.Json;

namespace ArithmeticCalculatorUserApi.Application.Helpers
{
    public static class ResponseHelper
    {
        public static APIGatewayProxyResponse BuildResponse(HttpStatusCode statusCode, object body) =>
           new()
           {
               StatusCode = (int)statusCode,
               Headers = CorsHelper.GetCorsHeaders(),
               Body = JsonSerializer.Serialize(new ApiResponse
               {
                   Data = body,
                   StatusCode = (int)statusCode
               })
           };

        public static T ParseRequestOrThrow<T>(string requestBody)
        {
            if (!RequestParserHelper.TryParseRequest<T>(requestBody, out var parsedRequest, out var errorMessage))
                throw new HttpResponseException(HttpStatusCode.BadRequest, errorMessage!);

            return parsedRequest!;
        }
    }
}
