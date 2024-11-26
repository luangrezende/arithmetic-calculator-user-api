using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ArithmeticCalculatorUserApi.Domain.Constants;
using ArithmeticCalculatorUserApi.Domain.Enums;
using ArithmeticCalculatorUserApi.Domain.Models.Request;
using ArithmeticCalculatorUserApi.Domain.Models.Response;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using ArithmeticCalculatorUserApi.Domain.Services;
using ArithmeticCalculatorUserApi.Domain.Services.Interfaces;
using ArithmeticCalculatorUserApi.Helpers;
using ArithmeticCalculatorUserApi.Infrastructure.Repositories;
using ArithmeticCalculatorUserApi.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ArithmeticCalculatorUserApi;

public class Function
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JwtTokenValidator _jwtTokenValidator;

    public Function()
    {
        var jwtSecret = Environment.GetEnvironmentVariable("jwtSecretKey");
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        _jwtTokenValidator = new JwtTokenValidator(jwtSecret!);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped(sp => new JwtTokenGenerator(Environment.GetEnvironmentVariable("jwtSecretKey")!));
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (request.HttpMethod == "OPTIONS")
            return BuildPreflightResponse();

        return request.HttpMethod switch
        {
            "POST" when request.Path == "/user/login" => await Login(request),
            "POST" when request.Path == "/user/register" => await Register(request),
            "GET" when request.Path == "/user/profile" => await GetProfile(request),
            "POST" when request.Path == "/account/add-balance" => await AddBalance(request),
            "POST" when request.Path == "/account/debit-balance" => await DebitBalance(request),
            _ => BuildResponse(HttpStatusCode.NotFound, new { error = ApiResponseMessages.EndpointNotFound }),
        };
    }

    private async Task<APIGatewayProxyResponse> Login(APIGatewayProxyRequest request)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();
        var jwtTokenGenerator = _serviceProvider.GetRequiredService<JwtTokenGenerator>();

        if (!RequestParserHelper.TryParseRequest<TokenRequest>(request.Body, out var user, out var errorMessage))
            return BuildResponse(HttpStatusCode.BadRequest, errorMessage!);

        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            return BuildResponse(HttpStatusCode.BadRequest, new { error = ApiResponseMessages.MissingUsernameOrPassword });

        var result = await userService.AuthenticateAsync(user.Username, user.Password);
        if (result == null)
            return BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiResponseMessages.InvalidCredentials });

        if (!result.Status.Equals(UserStatus.Active.ToString(), StringComparison.CurrentCultureIgnoreCase))
            return BuildResponse(HttpStatusCode.Forbidden, new { error = ApiResponseMessages.UserInactive });

        var token = jwtTokenGenerator.GenerateToken(result);

        return BuildResponse(HttpStatusCode.OK, new TokenResponse
        {
            Token = token,
            Expiration = (int)TokenConfiguration.ExpirationTimeInSeconds,
        });
    }

    private async Task<APIGatewayProxyResponse> Register(APIGatewayProxyRequest request)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();

        if (!RequestParserHelper.TryParseRequest<UserCreationRequest>(request.Body, out var user, out var errorMessage))
            return BuildResponse(HttpStatusCode.BadRequest, errorMessage!);

        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Name))
            return BuildResponse(HttpStatusCode.BadRequest, new { error = ApiResponseMessages.UsernamePasswordNameRequired });

        if (await userService.UserExistsAsync(user.Username))
            return BuildResponse(HttpStatusCode.Conflict, new { error = ApiResponseMessages.UsernameAlreadyExists });

        if (!await userService.CreateUserAsync(user.Username, user.Password, user.Name))
            return BuildResponse(HttpStatusCode.InternalServerError, new { error = ApiResponseMessages.ErrorCreatingUser });

        return BuildResponse(HttpStatusCode.Created, new { message = ApiResponseMessages.UserCreatedSuccessfully });
    }

    private async Task<APIGatewayProxyResponse> GetProfile(APIGatewayProxyRequest request)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();
        var bankAccountService = _serviceProvider.GetRequiredService<IBankAccountService>();

        if (!TryValidateToken(request, out var userId))
            return BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiResponseMessages.InvalidToken });

        var user = await userService.GetUserByIdAsync(userId);
        if (user == null)
            return BuildResponse(HttpStatusCode.NotFound, new { error = ApiResponseMessages.UserNotFound });

        var accounts = await bankAccountService.GetBankAccountsByUserIdAsync(userId);

        return BuildResponse(HttpStatusCode.OK, new UserProfileResponse 
        { 
            Id = user.Id, 
            Username = user.Username,
            Name = user.Name, 
            Status = user.Status, 
            Accounts = accounts });
    }

    private async Task<APIGatewayProxyResponse> AddBalance(APIGatewayProxyRequest request)
    {
        var bankAccountService = _serviceProvider.GetRequiredService<IBankAccountService>();

        if (!TryValidateToken(request, out var userId))
            return BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiResponseMessages.InvalidToken });

        if (!RequestParserHelper.TryParseRequest<AddBalanceRequest>(request.Body, out var addBalanceRequest, out var errorMessage))
            return BuildResponse(HttpStatusCode.BadRequest, errorMessage!);

        if (!await bankAccountService.AccountBelongsToUserAsync(addBalanceRequest.AccountId, userId))
            return BuildResponse(HttpStatusCode.Forbidden, new { error = ApiResponseMessages.AccountNotBelongToUser });

        if (!await bankAccountService.AddBalanceAsync(addBalanceRequest.AccountId, addBalanceRequest.Amount))
            return BuildResponse(HttpStatusCode.InternalServerError, new { error = ApiResponseMessages.AddBalanceFailed });

        return BuildResponse(HttpStatusCode.OK, new ApiResponse 
        { 
            Data = new 
            { 
                message = ApiResponseMessages.AddBalanceSuccess 
            },
            StatusCode = (int)HttpStatusCode.OK
        });
    }

    private async Task<APIGatewayProxyResponse> DebitBalance(APIGatewayProxyRequest request)
    {
        var bankAccountService = _serviceProvider.GetRequiredService<IBankAccountService>();

        if (!TryValidateToken(request, out var userId))
            return BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiResponseMessages.InvalidToken });

        if (!RequestParserHelper.TryParseRequest<DebitBalanceRequest>(request.Body, out var debitBalanceRequest, out var errorMessage))
            return BuildResponse(HttpStatusCode.BadRequest, errorMessage!);

        if (!await bankAccountService.AccountBelongsToUserAsync(debitBalanceRequest!.AccountId, userId))
            return BuildResponse(HttpStatusCode.Forbidden, new { error = ApiResponseMessages.AccountNotBelongToUser });

        if (!await bankAccountService.DebitBalanceAsync(debitBalanceRequest.AccountId, debitBalanceRequest.Amount))
            return BuildResponse(HttpStatusCode.BadRequest, new { error = ApiResponseMessages.InsufficientBalance });

        return BuildResponse(HttpStatusCode.OK, new { message = ApiResponseMessages.DebitBalanceSuccess });
    }

    private bool TryValidateToken(APIGatewayProxyRequest request, out Guid userId)
    {
        if (!request.Headers.TryGetValue("Authorization", out var authorization) || string.IsNullOrWhiteSpace(authorization))
        {
            userId = Guid.Empty;
            return false;
        }

        var token = authorization.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        return _jwtTokenValidator.ValidateToken(token, out userId);
    }

    private APIGatewayProxyResponse BuildPreflightResponse() =>
        new()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Headers = GetCorsHeaders()
        };

    private APIGatewayProxyResponse BuildResponse(HttpStatusCode statusCode, object body) =>
        new()
        {
            StatusCode = (int)statusCode,
            Headers = GetCorsHeaders(),
            Body = JsonSerializer.Serialize(
                new ApiResponse
                {
                    Data = body,
                    StatusCode = (int)HttpStatusCode.OK,
                }),
        };

    private Dictionary<string, string> GetCorsHeaders() =>
        new()
        {
            { "Access-Control-Allow-Origin", "*" },
            { "Access-Control-Allow-Methods", "GET, POST, OPTIONS" },
            { "Access-Control-Allow-Headers", "Content-Type, Authorization" }
        };
}
