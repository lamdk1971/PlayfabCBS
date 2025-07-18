# Get list of subscriptions
Write-Host "Getting Azure subscriptions..."
$subscriptions = az account list | ConvertFrom-Json
if ($subscriptions.Length -eq 0) {
    Write-Host "No subscriptions found. Please login to Azure first using 'az login'"
    exit
}

# Display subscriptions
Write-Host "`nAvailable subscriptions:"
for ($i = 0; $i -lt $subscriptions.Length; $i++) {
    Write-Host "$($i + 1). $($subscriptions[$i].name) ($($subscriptions[$i].id))"
}

# Get subscription selection
do {
    $selection = Read-Host "`nEnter Subscription Name or ID for Function App"
    $selectedSubscription = $subscriptions | Where-Object { $_.name -eq $selection -or $_.id -eq $selection }
    
    if (-not $selectedSubscription) {
        Write-Host "Invalid selection. Please try again."
    }
} while (-not $selectedSubscription)

# Set the selected subscription
az account set --subscription $selectedSubscription.id

# Get Function App name
do {
    $functionAppName = Read-Host "`nEnter Function App name (use only letters, numbers, and hyphens)"
    if ($functionAppName -match '^[a-zA-Z0-9-]+$') {
        break
    }
    Write-Host "Invalid name. Please use only letters, numbers, and hyphens."
} while ($true)

# Create resource group
$resourceGroup = "$functionAppName-rg"
Write-Host "`nCreating resource group '$resourceGroup'..."
az group create --name $resourceGroup --location eastus

# Create storage account (name must be lowercase and no special characters)
$storageAccount = $functionAppName.ToLower() -replace '[^a-z0-9]', ''
if ($storageAccount.Length -gt 24) {
    $storageAccount = $storageAccount.Substring(0, 24)
}
Write-Host "`nCreating storage account '$storageAccount'..."
az storage account create --name $storageAccount --resource-group $resourceGroup --location eastus --sku Standard_LRS

# Create function app
Write-Host "`nCreating function app '$functionAppName'..."
az functionapp create --name $functionAppName --resource-group $resourceGroup --storage-account $storageAccount --runtime dotnet --functions-version 4 --os-type Windows --consumption-plan-location eastus

Write-Host "`nFunction App creation completed successfully!"
Write-Host "Resource Group: $resourceGroup"
Write-Host "Storage Account: $storageAccount"
Write-Host "Function App: $functionAppName" 