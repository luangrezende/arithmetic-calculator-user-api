# Unit Tests for Arithmetic Calculator User API

This document describes the comprehensive unit test suite for the Arithmetic Calculator User API application.

## Test Structure

The tests are organized following the application architecture:

### Tests Location
- **Domain Tests**: `tests/ArithmeticCalculatorUserApi.Domain.Tests/`
- **Infrastructure Tests**: `tests/ArithmeticCalculatorUserApi.Infrastructure.Tests/`

### Test Categories

#### 1. Service Layer Tests (`/Services`)
- **UserServiceTests**: Tests for user authentication, creation, and retrieval
- **BankAccountServiceTests**: Tests for bank account operations
- **RefreshTokenServiceTests**: Tests for refresh token management
- **SecurityServiceTests**: Tests for password hashing and verification
- **TokenGeneratorServiceTests**: Tests for JWT token generation

#### 2. Handler Layer Tests (`/Handlers`)
- **UserHandlerTests**: Tests for API Gateway request handling

#### 3. Repository Layer Tests (`/Repositories`)
- **UserRepositoryTests**: Tests for user data access
- **BankAccountRepositoryTests**: Tests for bank account data access

#### 4. DTO Tests (`/DTOs`)
- **UserDTOTests**: Tests for user data transfer objects
- **BankAccountDTOTests**: Tests for bank account data transfer objects
- **RefreshTokenDTOTests**: Tests for refresh token data transfer objects

## Test Framework and Tools

- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependencies
- **BCrypt.Net**: For password hashing in tests
- **.NET 8**: Target framework

## Running Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests for specific project
dotnet test tests/ArithmeticCalculatorUserApi.Domain.Tests/
dotnet test tests/ArithmeticCalculatorUserApi.Infrastructure.Tests/

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Visual Studio
1. Open Test Explorer (`Test` → `Test Explorer`)
2. Click "Run All Tests" or run specific test classes/methods

### VS Code
1. Install C# extension
2. Install .NET Core Test Explorer extension
3. Use Test Explorer panel to run tests

## Test Coverage

The test suite covers:

### ✅ Covered Components
- **UserService**: Authentication, user operations, validation
- **BankAccountService**: Balance operations, account management
- **RefreshTokenService**: Token lifecycle management
- **SecurityService**: Password operations
- **TokenGeneratorService**: JWT generation and validation
- **UserHandler**: API request handling, endpoint routing
- **Repository Layer**: Data access operations
- **DTOs**: Data transfer object validation

### Test Patterns Used
- **AAA Pattern**: Arrange, Act, Assert
- **Theory Tests**: Multiple input variations using `[InlineData]`
- **Mocking**: All external dependencies mocked
- **Exception Testing**: Negative scenarios and error handling
- **Edge Cases**: Empty/null values, boundary conditions

### Example Test Scenarios
```csharp
// Positive scenario
[Fact]
public async Task AuthenticateAsync_ShouldReturnUserDTO_WhenCredentialsAreValid()

// Negative scenario  
[Fact]
public async Task AuthenticateAsync_ShouldReturnNull_WhenUserNotFound()

// Edge cases with Theory
[Theory]
[InlineData("")]
[InlineData(" ")]
[InlineData(null)]
public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenUsernameIsInvalid(string username)
```

## Test Configuration

### Dependencies
Tests use mocked dependencies:
- `IUserRepository` → `Mock<IUserRepository>`
- `ISecurityService` → `Mock<ISecurityService>`
- `IDbConnectionService` → `Mock<IDbConnectionService>`

### Environment Setup
- No external databases required
- No network calls made
- All tests run in isolation
- Fast execution (unit tests only)

## Continuous Integration

The tests are designed to run in CI/CD pipelines:
- No external dependencies
- Deterministic results
- Fast execution time
- Clear failure messages

## Adding New Tests

When adding new functionality:

1. **Service Tests**: Test business logic, validation, error handling
2. **Repository Tests**: Test data access, parameter validation
3. **Handler Tests**: Test API routing, request/response handling
4. **DTO Tests**: Test data structure validation

### Test Naming Convention
```
MethodName_Condition_ExpectedResult
```

Examples:
- `CreateUserAsync_ShouldReturnTrue_WhenValidDataProvided`
- `AuthenticateAsync_ShouldReturnNull_WhenPasswordIsInvalid`
- `GetUserByIdAsync_ShouldThrowException_WhenIdIsEmpty`

## Debugging Tests

### Common Issues
1. **Mock Setup**: Ensure all required mocks are configured
2. **Async/Await**: Use proper async patterns in test methods
3. **Data Setup**: Verify test data matches expected values
4. **Exception Testing**: Use `Assert.ThrowsAsync` for async methods

### Tips
- Use descriptive test names
- Keep tests simple and focused
- One assertion per test method
- Use meaningful test data
- Mock all external dependencies

This comprehensive test suite ensures the reliability and maintainability of the Arithmetic Calculator User API.
