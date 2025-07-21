# Script for AWS Lambda initial setup
# Run this script to create necessary AWS resources

param(
    [Parameter(Mandatory=$true)]
    [string]$AccountId,
    
    [Parameter(Mandatory=$false)]
    [string]$Region = "us-east-1",
    
    [Parameter(Mandatory=$false)]
    [string]$RoleName = "ArithmeticCalculatorUserLambdaRole"
)

Write-Host "Setting up AWS resources for Arithmetic Calculator User API project..." -ForegroundColor Green

# Check if AWS CLI is installed
try {
    aws --version | Out-Null
    Write-Host "✓ AWS CLI detected" -ForegroundColor Green
} catch {
    Write-Error "AWS CLI not found. Please install AWS CLI first."
    exit 1
}

# Check if authenticated
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    Write-Host "✓ Authenticated as: $($identity.Arn)" -ForegroundColor Green
} catch {
    Write-Error "Could not verify AWS identity. Run 'aws configure' first."
    exit 1
}

# Create Lambda trust policy
$trustPolicy = @{
    Version = "2012-10-17"
    Statement = @(
        @{
            Effect = "Allow"
            Principal = @{
                Service = "lambda.amazonaws.com"
            }
            Action = "sts:AssumeRole"
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "Creating Lambda execution role..." -ForegroundColor Yellow

# Save trust policy to temp file
$trustPolicyFile = [System.IO.Path]::GetTempFileName()
$trustPolicy | Out-File -FilePath $trustPolicyFile -Encoding UTF8

try {
    # Create role
    aws iam create-role `
        --role-name $RoleName `
        --assume-role-policy-document "file://$trustPolicyFile" `
        --description "Execution role for Arithmetic Calculator User API Lambda"
    
    Write-Host "✓ Role created: $RoleName" -ForegroundColor Green
} catch {
    Write-Host "Role already exists or creation error. Continuing..." -ForegroundColor Yellow
}

# Attach basic Lambda execution policy
Write-Host "Attaching basic Lambda execution policy..." -ForegroundColor Yellow
aws iam attach-role-policy `
    --role-name $RoleName `
    --policy-arn "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"

# Wait for role propagation
Write-Host "Waiting for role propagation..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Display important information
Write-Host "`n=== IMPORTANT INFORMATION ===" -ForegroundColor Cyan
Write-Host "Role ARN: arn:aws:iam::${AccountId}:role/$RoleName" -ForegroundColor White
Write-Host "Region: $Region" -ForegroundColor White

Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Cyan
Write-Host "1. Configure the following secrets on GitHub:" -ForegroundColor White
Write-Host "   - AWS_ACCESS_KEY_ID" -ForegroundColor Gray
Write-Host "   - AWS_SECRET_ACCESS_KEY" -ForegroundColor Gray
Write-Host "   - LAMBDA_EXECUTION_ROLE_ARN: arn:aws:iam::${AccountId}:role/$RoleName" -ForegroundColor Gray
Write-Host "   - MYSQL_CONNECTION_STRING" -ForegroundColor Gray
Write-Host "   - JWT_SECRET_KEY" -ForegroundColor Gray

Write-Host "`n2. Push to main branch to trigger deployment" -ForegroundColor White

Write-Host "`n=== USEFUL COMMANDS ===" -ForegroundColor Cyan
Write-Host "Check created role:" -ForegroundColor White
Write-Host "aws iam get-role --role-name $RoleName" -ForegroundColor Gray

Write-Host "`nCheck attached policies:" -ForegroundColor White
Write-Host "aws iam list-attached-role-policies --role-name $RoleName" -ForegroundColor Gray

# Clean temp files
Remove-Item $trustPolicyFile -Force

Write-Host "`n✓ Setup completed!" -ForegroundColor Green
