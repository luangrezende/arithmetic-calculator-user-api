using System.Net;
using System.Text.Json;
using ArithmeticCalculatorUserApi.Domain.Constants;
using ArithmeticCalculatorUserApi.Domain.Models.Response;

namespace ArithmeticCalculatorUserApi.Helpers
{
    public static class RequestParserHelper
    {
        public static bool TryParseRequest<T>(string requestBody, out T parsedObject, out ApiResponse errorResponse)
        {
            if (string.IsNullOrEmpty(requestBody))
            {
                parsedObject = default!;
                errorResponse = ApiResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.MissingBody);
                return false;
            }

            try
            {
                parsedObject = JsonSerializer.Deserialize<T>(requestBody)!;
                if (parsedObject == null)
                {
                    errorResponse = ApiResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.InvalidRequestBody);
                    return false;
                }

                errorResponse = null!;
                return true;
            }
            catch (JsonException)
            {
                parsedObject = default!;
                errorResponse = ApiResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.InvalidJsonFormat);
                return false;
            }
        }
    }
}
