using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace CBS.SignalR
{
    /// <summary>
    /// Configuration cho Azure SignalR Service
    /// </summary>
    public static class AzureSignalRConfig
    {
        // ========================================
        // AZURE SIGNALR SERVICE CONFIGURATION
        // ========================================

        /// <summary>
        /// Azure SignalR Service Connection String
        /// Format: "Endpoint=https://your-service.service.signalr.net;AccessKey=your-access-key;Version=1.0;"
        /// </summary>
        public static string SignalRConnectionString { get; set; } = 
            "Endpoint=https://your-signalr-service.service.signalr.net;AccessKey=your-access-key;Version=1.0;";

        /// <summary>
        /// Azure SignalR Service Hub Name
        /// </summary>
        public static string HubName { get; set; } = "ChatHub";

        /// <summary>
        /// Azure SignalR Service URL
        /// </summary>
        public static string SignalRServiceUrl { get; set; } = 
            "https://your-signalr-service.service.signalr.net";

        // ========================================
        // AUTHENTICATION CONFIGURATION
        // ========================================

        /// <summary>
        /// JWT Token Secret Key (for generating access tokens)
        /// </summary>
        public static string JwtSecretKey { get; set; } = "your-jwt-secret-key-here";

        /// <summary>
        /// JWT Token Issuer
        /// </summary>
        public static string JwtIssuer { get; set; } = "your-app-issuer";

        /// <summary>
        /// JWT Token Audience
        /// </summary>
        public static string JwtAudience { get; set; } = "your-app-audience";

        /// <summary>
        /// JWT Token Expiration Time (in minutes)
        /// </summary>
        public static int JwtExpirationMinutes { get; set; } = 60;

        // ========================================
        // CHAT CONFIGURATION
        // ========================================

        /// <summary>
        /// Maximum number of messages to keep in memory
        /// </summary>
        public static int MaxCachedMessages { get; set; } = 1000;

        /// <summary>
        /// Maximum number of users per chat room
        /// </summary>
        public static int MaxUsersPerChat { get; set; } = 1000;

        /// <summary>
        /// Message rate limiting (messages per second per user)
        /// </summary>
        public static int MaxMessagesPerSecond { get; set; } = 10;

        /// <summary>
        /// Enable message persistence to database
        /// </summary>
        public static bool EnableMessagePersistence { get; set; } = true;

        // ========================================
        // CONNECTION CONFIGURATION
        // ========================================

        /// <summary>
        /// Connection timeout (in seconds)
        /// </summary>
        public static int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Enable automatic reconnection
        /// </summary>
        public static bool EnableAutoReconnect { get; set; } = true;

        /// <summary>
        /// Maximum reconnection attempts
        /// </summary>
        public static int MaxReconnectAttempts { get; set; } = 5;

        /// <summary>
        /// Reconnection delay (in seconds)
        /// </summary>
        public static int ReconnectionDelaySeconds { get; set; } = 5;

        // ========================================
        // LOGGING CONFIGURATION
        // ========================================

        /// <summary>
        /// Enable SignalR logging
        /// </summary>
        public static bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Log level for SignalR operations
        /// </summary>
        public static LogLevel SignalRLogLevel { get; set; } = LogLevel.Information;

        // ========================================
        // SECURITY CONFIGURATION
        // ========================================

        /// <summary>
        /// Enable CORS for SignalR
        /// </summary>
        public static bool EnableCors { get; set; } = true;

        /// <summary>
        /// Allowed CORS origins
        /// </summary>
        public static string[] AllowedCorsOrigins { get; set; } = new string[]
        {
            "https://your-unity-app.com",
            "https://localhost:3000",
            "https://localhost:8080"
        };

        /// <summary>
        /// Enable message encryption
        /// </summary>
        public static bool EnableMessageEncryption { get; set; } = false;

        // ========================================
        // PERFORMANCE CONFIGURATION
        // ========================================

        /// <summary>
        /// Enable message compression
        /// </summary>
        public static bool EnableMessageCompression { get; set; } = true;

        /// <summary>
        /// Message compression threshold (bytes)
        /// </summary>
        public static int CompressionThresholdBytes { get; set; } = 1024;

        /// <summary>
        /// Enable connection pooling
        /// </summary>
        public static bool EnableConnectionPooling { get; set; } = true;

        /// <summary>
        /// Maximum pool size
        /// </summary>
        public static int MaxPoolSize { get; set; } = 100;

        // ========================================
        // MONITORING CONFIGURATION
        // ========================================

        /// <summary>
        /// Enable Azure Application Insights
        /// </summary>
        public static bool EnableApplicationInsights { get; set; } = true;

        /// <summary>
        /// Application Insights Instrumentation Key
        /// </summary>
        public static string AppInsightsInstrumentationKey { get; set; } = 
            "your-app-insights-instrumentation-key";

        // ========================================
        // DEPLOYMENT CONFIGURATION
        // ========================================

        /// <summary>
        /// Environment (Development, Staging, Production)
        /// </summary>
        public static string Environment { get; set; } = "Development";

        /// <summary>
        /// Azure SignalR Service SKU (Free, Standard, Premium)
        /// </summary>
        public static string SignalRServiceSku { get; set; } = "Standard";

        /// <summary>
        /// Azure SignalR Service Capacity (1-100)
        /// </summary>
        public static int SignalRServiceCapacity { get; set; } = 1;

        // ========================================
        // CONFIGURATION METHODS
        // ========================================

        /// <summary>
        /// Load configuration from appsettings.json
        /// </summary>
        public static void LoadFromConfiguration(IConfiguration configuration)
        {
            SignalRConnectionString = configuration["AzureSignalR:ConnectionString"] ?? SignalRConnectionString;
            HubName = configuration["AzureSignalR:HubName"] ?? HubName;
            SignalRServiceUrl = configuration["AzureSignalR:ServiceUrl"] ?? SignalRServiceUrl;
            
            JwtSecretKey = configuration["Authentication:JwtSecretKey"] ?? JwtSecretKey;
            JwtIssuer = configuration["Authentication:JwtIssuer"] ?? JwtIssuer;
            JwtAudience = configuration["Authentication:JwtAudience"] ?? JwtAudience;
            
            if (int.TryParse(configuration["Authentication:JwtExpirationMinutes"], out int jwtExp))
                JwtExpirationMinutes = jwtExp;

            if (int.TryParse(configuration["Chat:MaxCachedMessages"], out int maxCached))
                MaxCachedMessages = maxCached;

            if (int.TryParse(configuration["Chat:MaxUsersPerChat"], out int maxUsers))
                MaxUsersPerChat = maxUsers;

            if (int.TryParse(configuration["Chat:MaxMessagesPerSecond"], out int maxMsgs))
                MaxMessagesPerSecond = maxMsgs;

            if (bool.TryParse(configuration["Chat:EnableMessagePersistence"], out bool persistence))
                EnableMessagePersistence = persistence;

            Environment = configuration["Environment"] ?? Environment;
            SignalRServiceSku = configuration["AzureSignalR:Sku"] ?? SignalRServiceSku;
            
            if (int.TryParse(configuration["AzureSignalR:Capacity"], out int capacity))
                SignalRServiceCapacity = capacity;
        }

        /// <summary>
        /// Validate configuration
        /// </summary>
        public static bool ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(SignalRConnectionString))
            {
                throw new InvalidOperationException("Azure SignalR Connection String is required");
            }

            if (string.IsNullOrEmpty(JwtSecretKey))
            {
                throw new InvalidOperationException("JWT Secret Key is required");
            }

            if (string.IsNullOrEmpty(HubName))
            {
                throw new InvalidOperationException("SignalR Hub Name is required");
            }

            return true;
        }

        /// <summary>
        /// Get configuration summary for logging
        /// </summary>
        public static string GetConfigurationSummary()
        {
            return $@"
Azure SignalR Configuration:
- Environment: {Environment}
- Hub Name: {HubName}
- Service URL: {SignalRServiceUrl}
- Service SKU: {SignalRServiceSku}
- Service Capacity: {SignalRServiceCapacity}
- Max Cached Messages: {MaxCachedMessages}
- Max Users Per Chat: {MaxUsersPerChat}
- Max Messages Per Second: {MaxMessagesPerSecond}
- Enable Message Persistence: {EnableMessagePersistence}
- Enable Auto Reconnect: {EnableAutoReconnect}
- Enable Logging: {EnableLogging}
- Enable CORS: {EnableCors}
- Enable Message Compression: {EnableMessageCompression}
- Enable Application Insights: {EnableApplicationInsights}
";
        }
    }
} 