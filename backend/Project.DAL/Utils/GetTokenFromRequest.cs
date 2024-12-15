using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Project.DAL.Utils
{
    public class GetInfoFromToken
    {
        public static Guid? Id(HttpContext httpContext)
        {
            Claim? claim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (claim == null) return null;

            if (!Guid.TryParse(claim.Value, out Guid guidValue)) return null;

            return guidValue;
        }

        public static Guid? Id(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);

            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (claim == null) return null;

            if (!Guid.TryParse(claim.Value, out Guid guidValue)) return null;

            return guidValue;
        }
    }
}