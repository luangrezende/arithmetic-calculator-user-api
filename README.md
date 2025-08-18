# Arithmetic Calculator User API v1.0.0

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![AWS Lambda](https://img.shields.io/badge/AWS-Lambda-FF9900)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

A microservice responsible for user management, authentication, and account operations within the Arithmetic Calculator ecosystem. Built on AWS Lambda with .NET 8.

## Features

- **User Management**: Registration, authentication, and profile management
- **Account Management**: Create accounts and manage balances
- **Security**: JWT-based authentication with refresh token support
- **Serverless Architecture**: Deployed as AWS Lambda functions

## Architecture

This project follows Clean Architecture principles with a clear separation of concerns:

```
├── Domain         - Core business logic and entities
├── Application    - Use cases, DTOs and service interfaces
├── Infrastructure - External concerns (persistence, security)
└── Presentation   - API endpoints and request handling
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) (configured)
- [AWS Lambda Tools for .NET](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools)
  ```bash
  dotnet tool install -g Amazon.Lambda.Tools
  ```
- [Docker](https://www.docker.com/products/docker-desktop) (optional, for containerized development)

## Getting Started

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/luangrezende/arithmetic-calculator-user-api.git
   cd arithmetic-calculator-user-api
   ```

2. **Set up environment variables**
   ```bash
   # Windows (PowerShell)
   $env:JWT_SECRET_KEY="your-secret-key"
   $env:MYSQL_CONNECTION_STRING="Server=localhost;Database=calculator;User=root;Password=password;"
   $env:PROMOTIONAL_AMOUNT="10"

   # Linux/macOS
   export JWT_SECRET_KEY="your-secret-key"
   export MYSQL_CONNECTION_STRING="Server=localhost;Database=calculator;User=root;Password=password;"
   export PROMOTIONAL_AMOUNT="10"
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run locally using Lambda Test Tool**
   ```bash
   cd src/ArithmeticCalculatorUserApi.Presentation
   dotnet lambda run-server
   ```

The API will be accessible at `http://localhost:5000`.

### Docker Development

1. **Build the Docker image**
   ```bash
   docker build -t arithmetic-calculator-user-api .
   ```

2. **Run the container**
   ```bash
   docker run -p 5000:5000 \
     -e JWT_SECRET_KEY="your-secret-key" \
     -e MYSQL_CONNECTION_STRING="Server=host.docker.internal;Database=calculator;User=root;Password=password;" \
     -e PROMOTIONAL_AMOUNT="10" \
     arithmetic-calculator-user-api
   ```

## Deployment to AWS Lambda

### Manual Deployment

1. **Package the application**
   ```bash
   cd src/ArithmeticCalculatorUserApi.Presentation
   dotnet lambda package --configuration Release
   ```

2. **Deploy to AWS Lambda**
   ```bash
   dotnet lambda deploy-function ArithmeticCalculatorUserApi
   ```

### Automated Deployment with GitHub Actions

This project includes a CI/CD pipeline using GitHub Actions. To set up:

1. Add the following secrets to your GitHub repository:
   - `AWS_ACCESS_KEY_ID`
   - `AWS_SECRET_ACCESS_KEY`
   - `LAMBDA_EXECUTION_ROLE_ARN`
   - `MYSQL_CONNECTION_STRING`
   - `JWT_SECRET_KEY`

2. See [GitHub Actions Setup](docs/github-actions-setup.md) for detailed configuration.

## API Endpoints

### Health Check

- **GET /user/health** - Health check endpoint for monitoring

### Authentication

- **POST /v1/user/auth/register** - Register new user
- **POST /v1/user/auth/login** - User login
- **POST /v1/user/auth/refresh** - Refresh authentication token
- **POST /v1/user/auth/logout** - Logout (invalidate token)

### User Profile

- **GET /v1/user/profile** - Get authenticated user profile

### Account Management

- **POST /v1/user/account/balance** - Add funds to account
- **PUT /v1/user/account/balance** - Withdraw funds from account

## Testing

Run the test suite with:
```bash
dotnet test
```

The project includes unit tests for domain, application, and infrastructure layers.

## Configuration

The AWS Lambda configuration is in `aws-lambda-tools-defaults.json`:
```json
{
  "profile": "default",
  "region": "us-east-1",
  "configuration": "Release",
  "framework": "net8.0",
  "function-runtime": "dotnet8",
  "function-memory-size": 1024,
  "function-timeout": 60,
  "function-handler": "ArithmeticCalculatorUserApi.Presentation::ArithmeticCalculatorUserApi.Presentation.Function::FunctionHandler",
  "function-name": "ArithmeticCalculatorUserApi",
  "function-description": "Lambda function for Arithmetic Calculator User API",
  "package-type": "Zip"
}
```

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [AWS Lambda](https://aws.amazon.com/lambda/)
- [.NET Core](https://dotnet.microsoft.com/)
- [AWS Serverless Application Model](https://aws.amazon.com/serverless/sam/)
