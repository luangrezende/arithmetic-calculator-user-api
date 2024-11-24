namespace ArithmeticCalculatorUserApi.Domain.Constants
{
    public static class ErrorMessages
    {
        public const string MissingBody = "Request body cannot be null or empty.";
        public const string InvalidCredentials = "Invalid username or password.";
        public const string InternalServerError = "Internal server error.";
        public const string EndpointNotFound = "Endpoint not found.";
        public const string InvalidRequestBody = "Invalid request body.";
        public const string InvalidJsonFormat = "Invalid JSON format.";
        public const string MissingUsernameOrPassword = "Username and password are required.";
    }
}
