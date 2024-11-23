using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
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
            // Variáveis de ambiente
            _connectionString = Environment.GetEnvironmentVariable("mysql-connection-string");
            _jwtSecret = Environment.GetEnvironmentVariable("jwt-secret-key");
        }

        /// <summary>
        /// Gera um token JWT para o usuário autenticado.
        /// </summary>
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

        /// <summary>
        /// Endpoint para realizar login.
        /// </summary>
        public APIGatewayProxyResponse Login(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Body))
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 400,
                        Body = JsonSerializer.Serialize(new { message = "Request body cannot be null or empty." })
                    };
                }

                var user = JsonSerializer.Deserialize<User>(request.Body);

                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                {
                    return CreateResponse(400, new { message = "Username and password are required." });
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

                                return CreateResponse(200, new
                                {
                                    message = "Login successful!",
                                    token = token
                                });
                            }
                        }
                    }
                }

                return CreateResponse(401, new { message = "Invalid username or password." });
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error: {ex.Message}");
                return CreateResponse(500, new { message = "Internal server error." });
            }
        }

        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Path) || string.IsNullOrEmpty(request.HttpMethod))
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 400,
                        Body = JsonSerializer.Serialize(new { message = "Invalid request." })
                    };
                }

                if (request.HttpMethod == "POST" && request.Path == "/login")
                {
                    return Login(request, context);
                }

                return new APIGatewayProxyResponse
                {
                    StatusCode = 404,
                    Body = JsonSerializer.Serialize(new { message = "Endpoint not found." })
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error: {ex.Message}");
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = JsonSerializer.Serialize(new { message = "Internal server error." })
                };
            }
        }

        private static APIGatewayProxyResponse CreateResponse(int statusCode, object body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = JsonSerializer.Serialize(body),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
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
