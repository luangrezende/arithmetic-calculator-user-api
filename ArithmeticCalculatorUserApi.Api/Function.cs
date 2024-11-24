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

        public APIGatewayProxyResponse Login(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (!RequestParserHelper.TryParseRequest<TokenRequest>(request.Body, out var user, out var errorResponse))
            {
                return BuildResponse(HttpStatusCode.BadRequest, errorResponse.Data!);
            }

            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                return BuildResponse(HttpStatusCode.BadRequest, new { error = ApiResponseMessages.MissingUsernameOrPassword });
            }

            var result = _userRepository.Authenticate(user.Username, user.Password);
            if (!result.HasValue)
            {
                return BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiResponseMessages.InvalidCredentials });
            }

            var (userId, username, status) = result.Value;
            var token = _jwtTokenGenerator.GenerateToken(userId, username, status);

            return BuildResponse(HttpStatusCode.OK, new TokenResponse
            {
                Token = token,
                Validation = TokenExpirationHours * (int)TokenEnum.ExpirationTimeInSeconds
            });
        }

        public APIGatewayProxyResponse RegisterUser(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (!RequestParserHelper.TryParseRequest<UserCreationRequest>(request.Body, out var user, out var errorResponse))
            {
                return BuildResponse(HttpStatusCode.BadRequest, errorResponse.Data!);
            }

            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Name))
            {
                return BuildResponse(HttpStatusCode.BadRequest, new { error = ApiResponseMessages.UsernamePasswordNameRequired });
            }

            if (_userRepository.UserExists(user.Username))
            {
                return BuildResponse(HttpStatusCode.Conflict, new { error = ApiResponseMessages.UsernameAlreadyExists });
            }

            var created = _userRepository.CreateUser(user.Username, user.Password, user.Name);
            if (!created)
            {
                return BuildResponse(HttpStatusCode.InternalServerError, new { error = ApiResponseMessages.ErrorCreatingUser });
            }

            return BuildResponse(HttpStatusCode.Created, new { message = ApiResponseMessages.UserCreatedSuccessfully });
        }

        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (request.HttpMethod == "OPTIONS")
            {
                return BuildPreflightResponse();
            }

            return request.HttpMethod switch
            {
                "POST" when request.Path == "/login" => Login(request, context),
                "POST" when request.Path == "/register" => RegisterUser(request, context),
                _ => BuildResponse(HttpStatusCode.NotFound, new { error = ApiResponseMessages.EndpointNotFound }),
            };
        }

        private Dictionary<string, string> GetCorsHeaders()
        {
            return new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "GET, POST, OPTIONS" },
                { "Access-Control-Allow-Headers", "Content-Type, Authorization" }
            };
        }

        private APIGatewayProxyResponse BuildPreflightResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = GetCorsHeaders()
            };
        }

        private APIGatewayProxyResponse BuildResponse(HttpStatusCode statusCode, object body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)statusCode,
                Body = System.Text.Json.JsonSerializer.Serialize(body),
                Headers = GetCorsHeaders().Concat(new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }
    }
}
