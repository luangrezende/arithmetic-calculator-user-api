using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ArithmeticCalculatorUserApi.Application.Constants;
using ArithmeticCalculatorUserApi.Application.Helpers;
using ArithmeticCalculatorUserApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Application.Interfaces.Services;
using ArithmeticCalculatorUserApi.Application.Services;
using ArithmeticCalculatorUserApi.Infrastructure.Persistence;
using ArithmeticCalculatorUserApi.Infrastructure.Security;
using ArithmeticCalculatorUserApi.Presentation.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ArithmeticCalculatorUserApi.Presentation;

public class Function
{
    private readonly IServiceProvider _serviceProvider;

    public Function()
    {
        Logger.Initialize();
        
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var handler = new UserHandler(_serviceProvider);

        Logger.LogInformation("Processing request: {Method} {Path}", request.HttpMethod, request.Path);

        try
        {
            var response = await handler.HandleRequest(request);
            Logger.LogInformation("Request completed: {StatusCode}", response.StatusCode);
            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception processing request: {Method} {Path}", request.HttpMethod, request.Path);
            return HandleException(ex, context);
        }
    }

    private APIGatewayProxyResponse HandleException(Exception ex, ILambdaContext context)
    {
        Logger.LogError(ex, "Exception handled: {ExceptionType}", ex.GetType().Name);

        return ex switch
        {
            HttpResponseException httpEx => ResponseHelper.BuildResponse(httpEx.StatusCode, new { error = httpEx.Message ?? ApiErrorMessages.GenericError }),
            SecurityTokenException => ResponseHelper.BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiErrorMessages.InvalidToken }),
            SecurityTokenMalformedException => ResponseHelper.BuildResponse(HttpStatusCode.BadRequest, new { error = ApiErrorMessages.InvalidToken }),
            Exception => ResponseHelper.BuildResponse(HttpStatusCode.InternalServerError, new { error = ApiErrorMessages.InternalServerError }),
        };
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
}