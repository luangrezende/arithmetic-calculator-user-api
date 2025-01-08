using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using ArithmeticCalculatorUserApi.Application.Constants;
using ArithmeticCalculatorUserApi.Application.DTOs;
using ArithmeticCalculatorUserApi.Application.Helpers;
using ArithmeticCalculatorUserApi.Application.Interfaces.Services;
using ArithmeticCalculatorUserApi.Application.Models.Request;
using ArithmeticCalculatorUserApi.Application.Models.Response;
using ArithmeticCalculatorUserApi.Domain.Enums;
using ArithmeticCalculatorUserApi.Infrastructure.Enums;
using ArithmeticCalculatorUserApi.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace ArithmeticCalculatorUserApi.Presentation.Handlers;

public class UserHandler
{
    private readonly IServiceProvider _serviceProvider;

    public UserHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request)
    {
        return request.HttpMethod switch
        {
            "GET" when request.Path == "/v1/user/profile" => await GetProfile(request),
            "POST" when request.Path == "/v1/user/auth/login" => await Login(request),
            "POST" when request.Path == "/v1/user/auth/refresh" => await RefreshToken(request),
            "POST" when request.Path == "/v1/user/auth/logout" => await Logout(request),
            "POST" when request.Path == "/v1/user/auth/register" => await Register(request),
            "POST" when request.Path == "/v1/user/account/balance" => await AddBalance(request),
            "PUT" when request.Path == "/v1/user/account/balance" => await DebitBalance(request),
            _ => ResponseHelper.BuildResponse(HttpStatusCode.NotFound, new { error = ApiErrorMessages.EndpointNotFound })
        };
    }

    private async Task<APIGatewayProxyResponse> AddBalance(APIGatewayProxyRequest request)
    {
        var userId = ValidateTokenAndReturnUserId(request);
        var addBalanceRequest = ResponseHelper.ParseRequestOrThrow<UpdateBalanceRequest>(request.Body);

        if (addBalanceRequest.Amount <= (int)BalanceConfiguration.BalanceMinimumValue
            || addBalanceRequest.Amount > (int)BalanceConfiguration.BalanceMaximumValue)
        {
            return ResponseHelper.BuildResponse(HttpStatusCode.BadRequest, new
            {
                error = addBalanceRequest.Amount > (int)BalanceConfiguration.BalanceMaximumValue
                    ? ApiErrorMessages.ExceededMaximumAmount
                    : ApiErrorMessages.InvalidAmount
            });
        }

        await UpdateBalanceAsync(userId, addBalanceRequest.AccountId, addBalanceRequest.Amount, BalanceOperation.Add);

        return ResponseHelper.BuildResponse(HttpStatusCode.OK, new { message = ApiErrorMessages.AddBalanceSuccess });
    }

    private async Task<APIGatewayProxyResponse> DebitBalance(APIGatewayProxyRequest request)
    {
        var userId = ValidateTokenAndReturnUserId(request);
        var debitBalanceRequest = ResponseHelper.ParseRequestOrThrow<UpdateBalanceRequest>(request.Body);

        await UpdateBalanceAsync(userId, debitBalanceRequest.AccountId, debitBalanceRequest.Amount, BalanceOperation.Debit);

        return ResponseHelper.BuildResponse(HttpStatusCode.OK, new { message = ApiErrorMessages.DebitBalanceSuccess });
    }

    private async Task<APIGatewayProxyResponse> RefreshToken(APIGatewayProxyRequest request)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();
        var refreshTokenService = _serviceProvider.GetRequiredService<IRefreshTokenService>();
        var jwtTokenGenerator = _serviceProvider.GetRequiredService<ITokenGeneratorService>();

        var refreshTokenRequest = ResponseHelper.ParseRequestOrThrow<RefreshTokenRequest>(request.Body);

        var storedRefreshToken = await refreshTokenService.GetByTokenAsync(refreshTokenRequest.RefreshToken);
        if (storedRefreshToken == null || storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiErrorMessages.InvalidRefreshToken);

        await refreshTokenService.InvalidateTokenAsync(refreshTokenRequest.RefreshToken);

        var user = await userService.GetUserByIdAsync(storedRefreshToken.UserId)
            ?? throw new HttpResponseException(HttpStatusCode.NotFound, ApiErrorMessages.UserNotFound);

        if (!user.IsActive())
            throw new HttpResponseException(HttpStatusCode.Conflict, ApiErrorMessages.UserInactive);

        var newAccessToken = jwtTokenGenerator.GenerateToken(new UserDTO
        {
            Id = user.Id,
            Status = user.Status,
            Username = user.Username
        });

        var token = await refreshTokenService.AddAsync(user.Id);

        return ResponseHelper.BuildResponse(HttpStatusCode.OK, new TokenResponse
        {
            Token = newAccessToken,
            RefreshToken = token,
            Expiration = DateTime.UtcNow.AddSeconds((int)TokenConfiguration.AccessTokenExpirationTimeInSeconds)
        });
    }

    private async Task UpdateBalanceAsync(Guid userId, Guid accountId, decimal amount, BalanceOperation operation)
    {
        var bankAccountService = _serviceProvider.GetRequiredService<IBankAccountService>();

        if (!await bankAccountService.AccountBelongsToUserAsync(accountId, userId))
            throw new HttpResponseException(HttpStatusCode.Forbidden, ApiErrorMessages.AccountNotBelongToUser);

        bool success = operation switch
        {
            BalanceOperation.Add => await bankAccountService.AddBalanceAsync(accountId, amount),
            BalanceOperation.Debit => await bankAccountService.DebitBalanceAsync(accountId, amount),
            _ => throw new HttpResponseException(HttpStatusCode.BadRequest, ApiErrorMessages.InvalidOperation)
        };

        if (!success)
            throw new HttpResponseException(HttpStatusCode.BadRequest, operation == BalanceOperation.Add
                ? ApiErrorMessages.AddBalanceFailed
                : ApiErrorMessages.InsufficientBalance);
    }

    private async Task<APIGatewayProxyResponse> GetProfile(APIGatewayProxyRequest request)
    {
        var userId = ValidateTokenAndReturnUserId(request);
        var userService = _serviceProvider.GetRequiredService<IUserService>();
        var bankAccountService = _serviceProvider.GetRequiredService<IBankAccountService>();

        var user = await userService.GetUserByIdAsync(userId) 
            ?? throw new HttpResponseException(HttpStatusCode.NotFound, ApiErrorMessages.UserNotFound);
        var accounts = await bankAccountService.GetBankAccountsByUserIdAsync(userId);

        return ResponseHelper.BuildResponse(HttpStatusCode.OK, new UserProfileResponse
        {
            Username = user.Username,
            Name = user.Name,
            Status = user.Status,
            Accounts = accounts!.Select(account => new BankAccountResponse
            {
                Id = account.Id,
                Balance = account.Balance,
                Currency = account.Currency,
            }).ToList()
        });
    }

    private async Task<APIGatewayProxyResponse> Login(APIGatewayProxyRequest request)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();
        var refreshTokenService = _serviceProvider.GetRequiredService<IRefreshTokenService>();
        var jwtTokenGenerator = _serviceProvider.GetRequiredService<ITokenGeneratorService>();

        var loginRequest = ResponseHelper.ParseRequestOrThrow<TokenRequest>(request.Body);

        var user = await userService.AuthenticateAsync(loginRequest.Username, loginRequest.Password)
            ?? throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiErrorMessages.InvalidCredentials);

        if (!user.IsActive())
            throw new HttpResponseException(HttpStatusCode.Conflict, ApiErrorMessages.UserInactive);

        var accessToken = jwtTokenGenerator.GenerateToken(new UserDTO
        {
            Id = user.Id,
            Status = user.Status,
            Username = user.Username
        });

        var refreshToken = await refreshTokenService.AddAsync(user.Id);

        return ResponseHelper.BuildResponse(HttpStatusCode.OK, new TokenResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddSeconds((int)TokenConfiguration.AccessTokenExpirationTimeInSeconds)
        });
    }

    private async Task<APIGatewayProxyResponse> Logout(APIGatewayProxyRequest request)
    {
        var refreshTokenService = _serviceProvider.GetRequiredService<IRefreshTokenService>();
        var logoutRequest = ResponseHelper.ParseRequestOrThrow<RefreshTokenRequest>(request.Body);

        var isRevoked = await refreshTokenService.InvalidateTokenAsync(logoutRequest.RefreshToken);

        if (!isRevoked)
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiErrorMessages.InvalidToken);

        return ResponseHelper.BuildResponse(HttpStatusCode.OK, new { message = ApiErrorMessages.LogoutSuccessful });
    }

    private async Task<APIGatewayProxyResponse> Register(APIGatewayProxyRequest request)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();
        var userRegisterRequest = ResponseHelper.ParseRequestOrThrow<UserRegisterRequest>(request.Body);

        if (!userRegisterRequest.IsValid())
            throw new HttpResponseException(HttpStatusCode.BadRequest, ApiErrorMessages.UserPasswordMatchError);

        var userFound = await userService.GetUserByUsernameAsync(userRegisterRequest.Username);
        if (userFound != null)
            throw new HttpResponseException(HttpStatusCode.Conflict, ApiErrorMessages.UsernameAlreadyExists);

        if (!await userService.CreateUserAsync(userRegisterRequest.Username, userRegisterRequest.Password, userRegisterRequest.Name))
            throw new HttpResponseException(HttpStatusCode.InternalServerError, ApiErrorMessages.ErrorCreatingUser);

        return ResponseHelper.BuildResponse(HttpStatusCode.Created, new { message = ApiErrorMessages.UserCreatedSuccessfully });
    }

    private Guid ValidateTokenAndReturnUserId(APIGatewayProxyRequest request)
    {
        var jwtTokenValidator = _serviceProvider.GetRequiredService<JwtTokenValidator>();

        if (request.Headers == null || !request.Headers.TryGetValue("Authorization", out var authorization) || string.IsNullOrWhiteSpace(authorization))
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiErrorMessages.TokenMissing);

        var token = authorization.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(token))
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiErrorMessages.TokenMissing);

        if (!jwtTokenValidator.ValidateToken(token, out var userId))
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiErrorMessages.InvalidToken);

        return userId;
    }
}
