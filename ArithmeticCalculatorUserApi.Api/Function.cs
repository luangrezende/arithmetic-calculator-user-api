using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ArithmeticCalculatorUserApi.Domain.Models.Response;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ArithmeticCalculatorUserApi
{
    public class Function
    {
        private readonly string _connectionString;
        private readonly string _jwtSecret;

        public Function()
        {
            _connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString");
            _jwtSecret = Environment.GetEnvironmentVariable("jwtSecretKey");
        }

        private string GenerateJwtToken(int userId, string username, string status)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim("id", userId.ToString()),
                new Claim("status", status),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "arithmetic-calculator",
                audience: "arithmetic-calculator",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public object Login(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Body))
                {
                    return new
                    {
                        status = 400,
                        data = new { error = "Request body cannot be null or empty." }
                    };
                }

                var user = JsonSerializer.Deserialize<User>(request.Body);

                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                {
                    return new
                    {
                        status = 400,
                        data = new { error = "Username and password are required." }
                    };
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = "SELECT Id, Username, Status FROM User WHERE Username = @Username AND Password = @Password";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", user.Username);
                        cmd.Parameters.AddWithValue("@Password", user.Password); // Substituir por hash em produção.

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var userId = reader.GetInt32("Id");
                                var username = reader.GetString("Username");
                                var status = reader.GetString("Status");

                                var token = GenerateJwtToken(userId, username, status);

                                return new
                                {
                                    status = 200,
                                    data = new TokenResponse
                                    {
                                        Token = token,
                                        Validation = 7200 // 2 horas em segundos
                                    }
                                };
                            }
                        }
                    }
                }

                return new
                {
                    status = 401,
                    data = new { error = "Invalid username or password." }
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error: {ex.Message}");
                return new
                {
                    status = 500,
                    data = new { error = "Internal server error." }
                };
            }
        }

        public object FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (request.HttpMethod == "POST" && request.Path == "/login")
            {
                return Login(request, context);
            }

            return new
            {
                status = 404,
                data = new { error = "Endpoint not found." }
            };
        }
    }

    public class User
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
