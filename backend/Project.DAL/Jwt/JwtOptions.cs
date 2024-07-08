using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Project.DAL.Jwt
{
    public class JwtOptions
    {
        private readonly IConfiguration _configuration;

        public JwtOptions(IConfiguration configuration)
        {
            _configuration = configuration;
            GetValues();
        }

        public bool SaveToken { get; private set; }
        public bool RequireHttpsMetadata { get; private set; }
        public TimeSpan ExpirationAccessToken { get; private set; }
        public TimeSpan ExpirationRefreshToken { get; private set; }
        public TokenValidationParameters TokenParameters { get; private set; } = new TokenValidationParameters();

        private void GetValues()
        {
            try
            {
                SaveToken = bool.Parse(_configuration["JwtOptions:SaveToken"]!);
                RequireHttpsMetadata = bool.Parse(_configuration["JwtOptions:RequireHttpsMetadata"]!);
                ExpirationAccessToken = TimeSpan.Parse(_configuration["JwtOptions:ExpirationAccessToken"]!);
                ExpirationRefreshToken = TimeSpan.Parse(_configuration["JwtOptions:ExpirationRefreshToken"]!);
                TokenParameters = new TokenValidationParameters
                {
                    ValidateIssuer = bool.Parse(_configuration["JwtOptions:TokenValidationParameters:ValidateIssuer"]!),
                    ValidateAudience = bool.Parse(_configuration["JwtOptions:TokenValidationParameters:ValidateAudience"]!),
                    ValidateLifetime = bool.Parse(_configuration["JwtOptions:TokenValidationParameters:ValidateLifetime"]!),
                    ValidateIssuerSigningKey = bool.Parse(_configuration["JwtOptions:TokenValidationParameters:ValidateIssuerSigningKey"]!),
                    ValidIssuers = _configuration.GetSection("JwtOptions:TokenValidationParameters:ValidIssuers").Get<string[]>(),
                    ValidAudiences = _configuration.GetSection("JwtOptions:TokenValidationParameters:ValidAudiences").Get<string[]>(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtOptions:TokenValidationParameters:IssuerSigningKey"]!)),
                    ClockSkew = TimeSpan.Parse(_configuration["JwtOptions:TokenValidationParameters:ClockSkew"]!)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse JWT options from configuration.", ex);
            }
        }
    }
}