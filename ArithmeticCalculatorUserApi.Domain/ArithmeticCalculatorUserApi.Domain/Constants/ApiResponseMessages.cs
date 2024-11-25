namespace ArithmeticCalculatorUserApi.Domain.Constants
{
    public static class ApiResponseMessages
    {
        public const string MissingBody = "Request body cannot be null or empty.";
        public const string InvalidCredentials = "Invalid username or password.";
        public const string InternalServerError = "Internal server error.";
        public const string EndpointNotFound = "Endpoint not found.";
        public const string InvalidRequestBody = "Invalid request body.";
        public const string InvalidJsonFormat = "Invalid JSON format.";
        public const string MissingUsernameOrPassword = "Username and password are required.";
        public const string UsernamePasswordNameRequired = "Username, password, and name are required.";
        public const string UsernameAlreadyExists = "This email is already registered. Please use another email.";
        public const string ErrorCreatingUser = "Error creating the user.";
        public const string UserCreatedSuccessfully = "User created successfully.";
        public const string UserInactive = "The user account is inactive.";
    }
}
