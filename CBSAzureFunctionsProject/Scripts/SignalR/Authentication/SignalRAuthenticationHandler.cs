using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CBS.SignalR
{
    public interface ISignalRAuthenticationHandler
    {
        Task<bool> ValidateTokenAsync(string token);
        string GenerateToken(string userId, string userName);
    }

    public class SignalRAuthenticationHandler : ISignalRAuthenticationHandler
    {
        private readonly ILogger<SignalRAuthenticationHandler> _logger;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public SignalRAuthenticationHandler(ILogger<SignalRAuthenticationHandler> logger)
        {
            _logger = logger;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token is null or empty");
                    return false;
                }

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AzureSignalRConfig.JwtSecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = AzureSignalRConfig.JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = AzureSignalRConfig.JwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                
                if (principal != null)
                {
                    _logger.LogInformation("Token validated successfully");
                    return true;
                }

                _logger.LogWarning("Token validation failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        public string GenerateToken(string userId, string userName)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(AzureSignalRConfig.JwtSecretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new[]
                    {
                        new System.Security.Claims.Claim("userId", userId),
                        new System.Security.Claims.Claim("userName", userName),
                        new System.Security.Claims.Claim("name", userName)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(AzureSignalRConfig.JwtExpirationMinutes),
                    Issuer = AzureSignalRConfig.JwtIssuer,
                    Audience = AzureSignalRConfig.JwtAudience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation($"Token generated for user {userId}");
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token");
                throw;
            }
        }
    }
} 