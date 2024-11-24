using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ArithmeticCalculatorUserApi.Domain.Constants;
using ArithmeticCalculatorUserApi.Domain.Enums;
using ArithmeticCalculatorUserApi.Domain.Models.Request;
using ArithmeticCalculatorUserApi.Domain.Models.Response;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using ArithmeticCalculatorUserApi.Helpers;
using ArithmeticCalculatorUserApi.Infrastructure.Repositories;
using ArithmeticCalculatorUserApi.Infrastructure.Security;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ArithmeticCalculatorUserApi
{
    public class Function
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private const int TokenExpirationHours = 2;

        public Function()
        {
            var connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString");
            var jwtSecret = Environment.GetEnvironmentVariable("jwtSecretKey");

            _userRepository = new UserRepository(connectionString!);
            _jwtTokenGenerator = new JwtTokenGenerator(jwtSecret!);
        }

        public ApiResponse Login(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (!RequestParserHelper.TryParseRequest<TokenRequest>(request.Body, out var user, out var errorResponse))
            {
                return errorResponse;
            }

            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                return ApiResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.MissingUsernameOrPassword);
            }

            var result = _userRepository.Authenticate(user.Username, user.Password);
            if (!result.HasValue)
            {
                return ApiResponseHelper.CreateErrorResponse(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            }

            var (userId, username, status) = result.Value;
            var token = _jwtTokenGenerator.GenerateToken(userId, username, status);

            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Data = new TokenResponse
                {
                    Token = token,
                    Validation = TokenExpirationHours * (int)TokenEnum.ExpirationTimeInSeconds
                }
            };
        }

        public ApiResponse RegisterUser(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (!RequestParserHelper.TryParseRequest<UserCreationRequest>(request.Body, out var user, out var errorResponse))
            {
                return errorResponse;
            }

            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Name))
            {
                return ApiResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, "Username, password, and name are required.");
            }

            // Check if the user already exists
            if (_userRepository.UserExists(user.Username))
            {
                return ApiResponseHelper.CreateErrorResponse(HttpStatusCode.Conflict, "Username already exists.");
            }

            // Create the user
            var created = _userRepository.CreateUser(user.Username, user.Password, user.Name);
            if (!created)
            {
                return ApiResponseHelper.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error creating the user.");
            }

            return new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Data = new { message = "User created successfully." }
            };
        }


        public ApiResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return request.HttpMethod switch
            {
                "POST" when request.Path == "/login" => Login(request, context),
                "POST" when request.Path == "/register" => RegisterUser(request, context),
                _ => ApiResponseHelper.CreateErrorResponse(HttpStatusCode.NotFound, ErrorMessages.EndpointNotFound),
            };
        }

    }
}
