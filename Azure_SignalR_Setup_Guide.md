# Azure SignalR Service Setup Guide

## 📋 Tổng quan

Hướng dẫn này sẽ giúp bạn setup Azure SignalR Service cho hệ thống chat của bạn.

## 🚀 Bước 1: Tạo Azure SignalR Service

### 1.1 Đăng nhập Azure Portal
- Truy cập: https://portal.azure.com
- Đăng nhập với tài khoản Azure của bạn

### 1.2 Tạo SignalR Service
1. Click **"Create a resource"**
2. Tìm kiếm **"SignalR"**
3. Chọn **"SignalR Service"**
4. Click **"Create"**

### 1.3 Cấu hình SignalR Service

#### Basic Settings:
```
Resource group: [Chọn hoặc tạo mới]
SignalR service name: your-signalr-service-name
Region: [Chọn region gần nhất]
Pricing tier: Standard (hoặc Free cho test)
```

#### Network Settings:
```
Service mode: Default
```

#### Tags (Optional):
```
Environment: Development
Project: CBS-Chat
```

### 1.4 Lấy Connection String
1. Sau khi tạo xong, vào SignalR Service
2. Vào **"Keys"** trong menu bên trái
3. Copy **"Primary connection string"**

## 🔧 Bước 2: Cấu hình trong Code

### 2.1 Cập nhật appsettings.json

```json
{
  "Values": {
    "AzureSignalR:ConnectionString": "Endpoint=https://your-signalr-service.service.signalr.net;AccessKey=your-access-key;Version=1.0;",
    "AzureSignalR:HubName": "ChatHub",
    "AzureSignalR:ServiceUrl": "https://your-signalr-service.service.signalr.net",
    "AzureSignalR:Sku": "Standard",
    "AzureSignalR:Capacity": "1",
    
    "Authentication:JwtSecretKey": "your-super-secret-jwt-key-here-make-it-long-and-secure",
    "Authentication:JwtIssuer": "your-app-issuer",
    "Authentication:JwtAudience": "your-app-audience",
    "Authentication:JwtExpirationMinutes": "60"
  }
}
```

### 2.2 Cập nhật AzureSignalRConfig.cs

```csharp
public static string SignalRConnectionString { get; set; } = 
    "Endpoint=https://your-signalr-service.service.signalr.net;AccessKey=your-access-key;Version=1.0;";
```

## 🔐 Bước 3: Cấu hình Authentication

### 3.1 Tạo JWT Secret Key
```bash
# Tạo một secret key mạnh (ít nhất 32 ký tự)
openssl rand -base64 32
```

### 3.2 Cập nhật JWT Settings
```json
{
  "Authentication": {
    "JwtSecretKey": "your-generated-secret-key",
    "JwtIssuer": "https://your-app.com",
    "JwtAudience": "https://your-app.com",
    "JwtExpirationMinutes": "60"
  }
}
```

## 🌐 Bước 4: Cấu hình CORS

### 4.1 Cập nhật Allowed Origins
```csharp
public static string[] AllowedCorsOrigins { get; set; } = new string[]
{
    "https://your-unity-app.com",
    "https://localhost:3000",
    "https://localhost:8080",
    "http://localhost:3000",
    "http://localhost:8080"
};
```

## 📊 Bước 5: Cấu hình Monitoring

### 5.1 Tạo Application Insights
1. Tạo Application Insights resource trong Azure
2. Copy Instrumentation Key
3. Cập nhật trong appsettings.json:

```json
{
  "Monitoring": {
    "EnableApplicationInsights": "true",
    "AppInsightsInstrumentationKey": "your-app-insights-instrumentation-key"
  }
}
```

## 🔄 Bước 6: Deploy và Test

### 6.1 Deploy Azure Functions
```bash
# Publish to Azure
func azure functionapp publish your-function-app-name
```

### 6.2 Test Connection
```csharp
// Test trong Unity
var signalRModule = CBSModule.Get<CBSSignalRModule>();
signalRModule.Connect("https://your-signalr-service.service.signalr.net", "your-jwt-token");
```

## 📈 Bước 7: Monitoring và Scaling

### 7.1 Azure Portal Monitoring
- Vào SignalR Service trong Azure Portal
- Xem **"Metrics"** để monitor performance
- Xem **"Logs"** để debug issues

### 7.2 Scaling Configuration
```json
{
  "AzureSignalR": {
    "Sku": "Standard",
    "Capacity": "1"  // Tăng lên khi cần
  }
}
```

## 🔧 Troubleshooting

### Connection Issues
```bash
# Kiểm tra connection string
# Kiểm tra network connectivity
# Kiểm tra CORS settings
```

### Authentication Issues
```bash
# Kiểm tra JWT token
# Kiểm tra secret key
# Kiểm tra issuer/audience
```

### Performance Issues
```bash
# Monitor connection count
# Check message rate
# Review capacity settings
```

## 📋 Configuration Checklist

### ✅ Azure SignalR Service
- [ ] Tạo SignalR Service
- [ ] Copy Connection String
- [ ] Cấu hình SKU và Capacity

### ✅ Authentication
- [ ] Tạo JWT Secret Key
- [ ] Cấu hình Issuer/Audience
- [ ] Test JWT token generation

### ✅ CORS
- [ ] Cấu hình Allowed Origins
- [ ] Test cross-origin requests

### ✅ Monitoring
- [ ] Setup Application Insights
- [ ] Cấu hình logging
- [ ] Test monitoring

### ✅ Deployment
- [ ] Deploy Azure Functions
- [ ] Test SignalR connection
- [ ] Verify chat functionality

## 💰 Cost Estimation

### Free Tier
- 20 connections
- 20,000 messages/day
- 1 unit

### Standard Tier
- $0.74/unit/month
- 1,000 connections/unit
- 1,000,000 messages/unit/month

### Premium Tier
- $3.70/unit/month
- 1,000 connections/unit
- 1,000,000 messages/unit/month
- SLA 99.9%

## 🚀 Production Checklist

### Security
- [ ] Use strong JWT secret
- [ ] Enable HTTPS only
- [ ] Configure CORS properly
- [ ] Set up rate limiting

### Performance
- [ ] Monitor connection count
- [ ] Set appropriate capacity
- [ ] Enable message compression
- [ ] Configure connection pooling

### Monitoring
- [ ] Setup Application Insights
- [ ] Configure alerts
- [ ] Monitor costs
- [ ] Set up logging

### Backup
- [ ] Document configuration
- [ ] Backup connection strings
- [ ] Test disaster recovery

## 📞 Support

Nếu gặp vấn đề:
1. Kiểm tra Azure SignalR documentation
2. Xem logs trong Application Insights
3. Check Azure SignalR metrics
4. Contact Azure support nếu cần

## 🔗 Useful Links

- [Azure SignalR Documentation](https://docs.microsoft.com/en-us/azure/azure-signalr/)
- [SignalR Hub Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/hubs)
- [JWT Authentication](https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-authenticate-oauth)
- [CORS Configuration](https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-cors) 