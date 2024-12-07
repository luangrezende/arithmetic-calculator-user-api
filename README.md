# Arithmetic Calculator User API

This API manages user-related operations for the Arithmetic Calculator platform, developed using AWS Lambda and .NET 8.

## Prerequisites

Ensure the following software is installed on your machine:

1. **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **AWS CLI** - [Installation Instructions](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
3. **AWS Lambda Tools for .NET** - Install using:
   ```bash
   dotnet tool install -g Amazon.Lambda.Tools
   ```
4. **Docker** - [Install Docker](https://www.docker.com/products/docker-desktop)

---

## Running Locally

### 1. Clone the Repository

Clone the repository to your local machine:

```bash
git clone https://github.com/luangrezende/arithmetic-calculator-user-api.git
cd arithmetic-calculator-user-api
```

### 2. Restore Dependencies

Restore required NuGet packages:

```bash
dotnet restore
```

### 3. Run the API Locally

Use the AWS Lambda Test Tool to run the API locally:

```bash
dotnet lambda run-server
```

The API will be accessible at `http://localhost:5000`.

---

## Running with Docker

### 1. Build the Docker Image

Build the Docker image using the following command:

```bash
docker build -t arithmetic-calculator-user-api .
```

### 2. Run the Docker Container

Run the Docker container:

```bash
docker run -p 5000:5000 arithmetic-calculator-user-api
```

The API will now be available at `http://localhost:5000`.

---

## Project Structure

├── src/
│ ├── ArithmeticCalculatorUserApi.Presentation/ # Main API project
│ ├── ArithmeticCalculatorUserApi.Application/ # Application layer (services, DTOs, use cases)
│ ├── ArithmeticCalculatorUserApi.Domain/ # Domain logic
│ ├── ArithmeticCalculatorUserApi.Infrastructure/ # Infrastructure logic
├── tests/
│ ├── ArithmeticCalculatorUserApi.Domain.Tests/ # Unit tests
├── .github/workflows/ # CI/CD workflows
├── .gitignore
├── ArithmeticCalculatorUserApi.sln # Solution file
└── README.md

---

## Configuration

Update the `aws-lambda-tools-defaults.json` file as needed for your Lambda configuration. Example:

```json
{
  "function-name": "ArithmeticCalculatorUserApi",
  "function-handler": "ArithmeticCalculatorUserApi::ArithmeticCalculatorUserApi.Function::FunctionHandler",
  "framework": "net8.0",
  "memory-size": 256,
  "timeout": 30,
  "region": "us-east-1"
}
```

---

## Testing the API

Use tools like **Postman** or **curl** to test the API. Example with `curl`:

```bash
curl -X GET http://localhost:5000/api/users
```

---

## Resources

- [AWS Lambda for .NET](https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet.html)
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-quickstart.html)
- [Docker Documentation](https://docs.docker.com/get-started/)

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.
