using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR;
using CBS.SignalR;
using System;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Load configuration
        var configuration = context.Configuration;
        AzureSignalRConfig.LoadFromConfiguration(configuration);
        
        // Validate configuration
        try
        {
            AzureSignalRConfig.ValidateConfiguration();
            Console.WriteLine("Azure SignalR Configuration validated successfully");
            Console.WriteLine(AzureSignalRConfig.GetConfigurationSummary());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Configuration validation failed: {ex.Message}");
            throw;
        }

        // ========================================
        // AZURE SIGNALR SERVICE CONFIGURATION
        // ========================================
        
        // Add Azure SignalR Service
        services.AddSignalR()
            .AddAzureSignalR(options =>
            {
                options.ConnectionString = AzureSignalRConfig.SignalRConnectionString;
                options.ServerStickyMode = ServerStickyMode.Required;
                
                // Configure hub options
                options.Hubs.Add(new HubOptions
                {
                    HubName = AzureSignalRConfig.HubName,
                    EnableDetailedErrors = AzureSignalRConfig.Environment == "Development",
                    ClientTimeoutInterval = TimeSpan.FromSeconds(AzureSignalRConfig.ConnectionTimeoutSeconds),
                    KeepAliveInterval = TimeSpan.FromSeconds(15),
                    MaximumReceiveMessageSize = 32768, // 32KB
                    StreamBufferCapacity = 10
                });
            });

        // ========================================
        // AUTHENTICATION SERVICES
        // ========================================
        
        // Add JWT authentication
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = AzureSignalRConfig.JwtIssuer;
                options.Audience = AzureSignalRConfig.JwtAudience;
                options.RequireHttpsMetadata = AzureSignalRConfig.Environment == "Production";
            });

        // ========================================
        // SIGNALR HUB SERVICES
        // ========================================
        
        // Register SignalR Hub
        services.AddScoped<ChatHub>();
        
        // Add SignalR authentication handler
        services.AddScoped<ISignalRAuthenticationHandler, SignalRAuthenticationHandler>();

        // ========================================
        // CORS CONFIGURATION
        // ========================================
        
        if (AzureSignalRConfig.EnableCors)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("SignalRPolicy", policy =>
                {
                    policy.WithOrigins(AzureSignalRConfig.AllowedCorsOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });
        }

        // ========================================
        // LOGGING CONFIGURATION
        // ========================================
        
        if (AzureSignalRConfig.EnableLogging)
        {
            services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(AzureSignalRConfig.SignalRLogLevel);
                logging.AddConsole();
                logging.AddDebug();
            });
        }

        // ========================================
        // APPLICATION INSIGHTS
        // ========================================
        
        if (AzureSignalRConfig.EnableApplicationInsights)
        {
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = AzureSignalRConfig.AppInsightsInstrumentationKey;
            });
        }

        // ========================================
        // PERFORMANCE OPTIMIZATION
        // ========================================
        
        if (AzureSignalRConfig.EnableConnectionPooling)
        {
            services.AddSingleton<IConnectionManager, ConnectionManager>();
        }

        // ========================================
        // EXISTING CBS SERVICES
        // ========================================
        
        // Add your existing CBS services here
        // services.AddScoped<IChatDataProvider, AzureChatDataProvider>();
        // services.AddScoped<IChatModule, ChatModule>();
        
        Console.WriteLine("Azure SignalR Services configured successfully");
    })
    .Build();

host.Run();
