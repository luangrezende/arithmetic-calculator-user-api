using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ArithmeticCalculatorUserApi.Application.Constants;
using ArithmeticCalculatorUserApi.Application.Helpers;
using ArithmeticCalculatorUserApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Application.Interfaces.Services;
using ArithmeticCalculatorUserApi.Application.Models.Response;
using ArithmeticCalculatorUserApi.Application.Services;
using ArithmeticCalculatorUserApi.Infrastructure.Interfaces.Services;
using ArithmeticCalculatorUserApi.Infrastructure.Persistence;
using ArithmeticCalculatorUserApi.Infrastructure.Security;
using ArithmeticCalculatorUserApi.Presentation.Handlers;
using ArithmeticCalculatorUserApi.Presentation.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ArithmeticCalculatorUserApi.Presentation;

public class Function
{
    private readonly IServiceProvider _serviceProvider;

    public Function()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var handler = new UserHandler(_serviceProvider);

        try
        {
            return await handler.HandleRequest(request);
        }
        catch (HttpResponseException ex)
        {
            LogError(context, "HttpResponseException", ex);
            return BuildResponse(ex.StatusCode, new { error = ex.Message ?? ApiErrorMessages.GenericError });
        }
        catch (SecurityTokenException ex)
        {
            LogError(context, "SecurityTokenInvalidException", ex);
            return BuildResponse(HttpStatusCode.BadRequest, new { error = ApiErrorMessages.InvalidToken });
        }
        catch (SecurityTokenMalformedException ex)
        {
            LogError(context, "SecurityTokenInvalidException", ex);
            return BuildResponse(HttpStatusCode.BadRequest, new { error = ApiErrorMessages.InvalidToken });
        }
        catch (Exception ex)
        {
            LogError(context, "Unhandled Exception", ex);
            return BuildResponse(HttpStatusCode.InternalServerError, new { error = ApiErrorMessages.InternalServerError });
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<ITokenGeneratorService, TokenGeneratorService>();
        services.AddScoped<ISecurityService, SecurityService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IDbConnectionService>(provider =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
                                ?? throw new InvalidOperationException(ApiErrorMessages.ConnectionStringNotSet);
            return new MySqlConnectionService(connectionString);
        });

        services.AddScoped(sp => new JwtTokenValidator(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!));
    }

    private APIGatewayProxyResponse BuildResponse(HttpStatusCode statusCode, object body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)statusCode,
            Body = JsonSerializer.Serialize(new ApiResponse { Data = body, StatusCode = (int)statusCode }),
            Headers = CorsHelper.GetCorsHeaders()
        };
    }

    private void LogError(ILambdaContext context, string errorType, Exception ex)
    {
        context.Logger.LogError($"{errorType}: {ex.Message} \nStackTrace: {ex.StackTrace}");
    }
}
