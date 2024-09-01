using Microsoft.IdentityModel.Tokens;
using Project.DAL.Entities;
using Project.DAL.Repositories.Permission;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Project.DAL.Jwt
{
    public sealed class JwtProvider(JwtOptions options, IPermissionRepository permissionService) : IJwtProvider
    {
        private readonly IPermissionRepository _permissionService = permissionService;
        private readonly JwtOptions _options = options;

        public async Task<string> GenerateAccessToken(User user)
        {

            // building claims
            List<Claim> claims = new()
            {
                 new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            HashSet<string> permissions = await _permissionService.GetPermissionsAsync(user.Id);
            foreach (string permission in permissions) claims.Add(new Claim(CustomClaims.Permissions, permission));

            // building signingCredentials
            SigningCredentials signingCredentials = new(
                _options.TokenParameters.IssuerSigningKey,
                SecurityAlgorithms.HmacSha256
            );

            // building the token
            JwtSecurityToken token = new(
                issuer: _options.TokenParameters.ValidIssuers.FirstOrDefault(),
                audience: _options.TokenParameters.ValidAudiences.FirstOrDefault(),
                expires: DateTime.UtcNow.Add(_options.ExpirationAccessToken),
                claims: claims,
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken(User user)
        {

            // building claims
            List<Claim> claims = new()
            {
                 new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                 new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // building signingCredentials
            SigningCredentials signingCredentials = new(
                _options.TokenParameters.IssuerSigningKey,
                SecurityAlgorithms.HmacSha256
            );

            // building the token
            JwtSecurityToken token = new(
                issuer: _options.TokenParameters.ValidIssuer,
                audience: _options.TokenParameters.ValidAudience,
                expires: DateTime.UtcNow.Add(_options.ExpirationRefreshToken),
                claims: claims,
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}