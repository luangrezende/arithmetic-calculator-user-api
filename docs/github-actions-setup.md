# Simple GitHub Actions Setup for AWS Lambda Deploy

This document explains how to configure automatic application deployment to AWS Lambda using GitHub Actions (simplified version for test environment).

## GitHub Secrets Configuration

Go to your repository settings on GitHub: `Settings > Secrets and variables > Actions`

### Required Secrets

Configure only these 5 secrets:

#### AWS Credentials
- `AWS_ACCESS_KEY_ID`: AWS access key
- `AWS_SECRET_ACCESS_KEY`: AWS secret key
- `LAMBDA_EXECUTION_ROLE_ARN`: Lambda execution role ARN

#### Application Secrets
- `MYSQL_CONNECTION_STRING`: MySQL connection string
- `JWT_SECRET_KEY`: JWT secret key

## How It Works

### Trigger
- Deploy runs automatically when you **push to `main` branch**

### Process
1. Checkout code
2. Setup .NET 8
3. Install AWS Lambda Tools
4. Configure AWS credentials
5. Restore dependencies
6. Build and package application
7. Deploy to AWS Lambda

### Function Name
- Function will be created with name: `ArithmeticCalculatorUserApi`

## Quick AWS IAM Role Setup

Run this command to create the necessary role (replace `123456789012` with your Account ID):

```bash
# 1. Create trust-policy.json file
echo '{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}' > trust-policy.json

# 2. Create the role
aws iam create-role \
  --role-name ArithmeticCalculatorUserLambdaRole \
  --assume-role-policy-document file://trust-policy.json

# 3. Attach basic policy
aws iam attach-role-policy \
  --role-name ArithmeticCalculatorUserLambdaRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole

# 4. Use this ARN in LAMBDA_EXECUTION_ROLE_ARN secret:
# arn:aws:iam::123456789012:role/ArithmeticCalculatorUserLambdaRole
```

## Example Secret Values

```
AWS_ACCESS_KEY_ID=AKIAIOSFODNN7EXAMPLE
AWS_SECRET_ACCESS_KEY=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
LAMBDA_EXECUTION_ROLE_ARN=arn:aws:iam::123456789012:role/ArithmeticCalculatorUserLambdaRole
MYSQL_CONNECTION_STRING=Server=localhost;Database=calculator;User=root;Password=password;
JWT_SECRET_KEY=my-super-secure-jwt-secret-key
```

## How to Deploy

1. Configure the 5 secrets on GitHub
2. Push to `main` branch
3. Watch the deployment in repository `Actions`
4. The `ArithmeticCalculatorUserApi` function will be created/updated on AWS

## Troubleshooting

### Common Errors

1. **Role not found**: Check if the role ARN is correct in the secret
2. **Function timeout**: Function is configured for 60 seconds
3. **Database connection error**: Check the connection string in the secret
4. **JWT error**: Check if the JWT key is configured properly

### Useful Commands

```bash
# Check if function was created
aws lambda get-function --function-name ArithmeticCalculatorUserApi

# View function logs
aws logs describe-log-groups --log-group-name-prefix /aws/lambda/ArithmeticCalculatorUserApi

# Test function locally
cd src/ArithmeticCalculatorUserApi.Presentation
dotnet lambda package --configuration Release
```

Done! Your application is now configured for automatic deployment to AWS Lambda in a simple and straightforward way. 🚀
