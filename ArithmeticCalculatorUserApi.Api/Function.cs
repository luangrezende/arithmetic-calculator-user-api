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
        private readonly JwtTokenValidator _jwtTokenValidator;

        public Function()
        {
            var connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString");
            var jwtSecret = Environment.GetEnvironmentVariable("jwtSecretKey");

            _userRepository = new UserRepository(connectionString!);
            _jwtTokenGenerator = new JwtTokenGenerator(jwtSecret!);
            _jwtTokenGenerator = new JwtTokenGenerator(jwtSecret!);
        }

        public APIGatewayProxyResponse GetProfile(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                if (!request.Headers.TryGetValue("Authorization", out var authorization) || string.IsNullOrWhiteSpace(authorization))
                {
                    return BuildResponse(HttpStatusCode.Unauthorized, new { error = "Missing or invalid token" });
                }

                var token = authorization.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
                if (!_jwtTokenValidator.ValidateToken(token, out var userId))
                {
                    return BuildResponse(HttpStatusCode.Unauthorized, new { error = "Invalid token" });
                }

                var user = _userRepository.GetUserById(userId);
                if (user == null)
                {
                    return BuildResponse(HttpStatusCode.NotFound, new { error = "User not found" });
                }

                return BuildResponse(HttpStatusCode.OK, new
                {
                    user.Id,
                    user.Username,
                    user.Name,
                    user.Email,
                    user.Status
                });
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error: {ex.Message}");
                return BuildResponse(HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred" });
            }
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
            if (result == null)
            {
                return BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiResponseMessages.InvalidCredentials });
            }

            if (!result.Status.Equals(UserStatus.Active.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                return BuildResponse(HttpStatusCode.Forbidden, new { error = ApiResponseMessages.UserInactive });
            }

            var token = _jwtTokenGenerator.GenerateToken(result);

            return BuildResponse(HttpStatusCode.OK, new TokenResponse
            {
                Token = token,
                Validation = (int)TokenConfiguration.ExpirationTimeInSeconds
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
                "POST" when request.Path == $"user/login" => Login(request, context),
                "POST" when request.Path == $"user/register" => RegisterUser(request, context),
                "GET" when request.Path == $"user/profile" => GetProfile(request, context),
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
