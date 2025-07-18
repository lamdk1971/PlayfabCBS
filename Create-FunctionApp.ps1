# This script creates and configures an Azure Function App for PlayFab integration
# Prerequisites:
# - Azure CLI installed
# - Azure Functions Core Tools installed
# - .NET SDK installed
# - PowerShell 7+ installed
# - VS Code with Azure Functions extension installed

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$FunctionAppName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location,
    
    [Parameter(Mandatory=$true)]
    [string]$StorageAccountName,
    
    [Parameter(Mandatory=$true)]
    [string]$PlayFabTitleId,
    
    [Parameter(Mandatory=$true)]
    [string]$PlayFabSecretKey,
    
    [Parameter(Mandatory=$false)]
    [string]$Runtime = "dotnet",
    
    [Parameter(Mandatory=$false)]
    [string]$RuntimeVersion = "6.0",
    
    [Parameter(Mandatory=$false)]
    [string]$Sku = "Y1",
    
    [Parameter(Mandatory=$false)]
    [string]$PlanName = "$FunctionAppName-plan"
)

# Function to verify Function App creation
function Verify-FunctionApp {
    param (
        [string]$ResourceGroupName,
        [string]$FunctionAppName
    )
    
    Write-Host "`nVerifying Function App creation..."
    
    # Get Function App status
    $functionApp = az functionapp show `
        --name $FunctionAppName `
        --resource-group $ResourceGroupName `
        --output json | ConvertFrom-Json
    
    if ($functionApp) {
        Write-Host "Function App '$FunctionAppName' exists and is ready!"
        Write-Host "State: $($functionApp.state)"
        Write-Host "Default Hostname: $($functionApp.defaultHostName)"
        return $true
    } else {
        Write-Host "Function App creation failed or is not ready yet. Please check Azure Portal."
        return $false
    }
}

# Login check
Write-Host "Checking Azure CLI login status..."
$loginCheck = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Please login to Azure CLI first using 'az login'"
    exit 1
}

# Create Resource Group if it doesn't exist
Write-Host "Creating Resource Group if it doesn't exist..."
az group create --name $ResourceGroupName --location $Location

# Create Storage Account
Write-Host "Creating Storage Account..."
az storage account create `
    --name $StorageAccountName `
    --location $Location `
    --resource-group $ResourceGroupName `
    --sku Standard_LRS `
    --kind StorageV2

# Get Storage Account Connection String
$storageConnectionString = $(az storage account show-connection-string `
    --name $StorageAccountName `
    --resource-group $ResourceGroupName `
    --query connectionString `
    --output tsv)

# Create Function App with Consumption Plan
Write-Host "Creating Function App..."
az functionapp create `
    --name $FunctionAppName `
    --storage-account $StorageAccountName `
    --resource-group $ResourceGroupName `
    --consumption-plan-location $Location `
    --runtime $Runtime `
    --runtime-version $RuntimeVersion `
    --functions-version 4 `
    --os-type Windows

# Configure Function App Settings
Write-Host "Configuring Function App Settings..."
az functionapp config appsettings set `
    --name $FunctionAppName `
    --resource-group $ResourceGroupName `
    --settings `
    "PLAYFAB_TITLE_ID=$PlayFabTitleId" `
    "PLAYFAB_DEV_SECRET_KEY=$PlayFabSecretKey" `
    "AzureWebJobsStorage=$storageConnectionString"

# Enable Function App Managed Identity
Write-Host "Enabling Managed Identity..."
az functionapp identity assign `
    --name $FunctionAppName `
    --resource-group $ResourceGroupName

# Verify Function App Creation
Verify-FunctionApp -ResourceGroupName $ResourceGroupName -FunctionAppName $FunctionAppName

Write-Host "`nTo view your Function App in VS Code:"
Write-Host "1. Make sure you have the following VS Code extensions installed:"
Write-Host "   - Azure Account"
Write-Host "   - Azure Functions"
Write-Host "2. Click on the Azure icon in the VS Code sidebar"
Write-Host "3. If you don't see your Function App:"
Write-Host "   - Click the Refresh icon in the Azure Functions view"
Write-Host "   - Or sign out and sign back in using 'Azure: Sign Out' and 'Azure: Sign In' commands"
Write-Host "4. Expand the Function App node to see your Function App"
Write-Host "`nFunction App URL: https://$FunctionAppName.azurewebsites.net"
Write-Host "Remember to register your functions in the PlayFab Game Manager under Automation > Cloud Script > Functions" 