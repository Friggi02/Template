using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Project.DAL.Utils
{
    public class GetInfoFromToken
    {
        public static Result<Guid> Id(HttpContext httpContext)
        {
            Claim? claim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (claim == null) return Result<Guid>.Failure(GetTokenFromRequestErrors.ClaimNotFound);

            if (!Guid.TryParse(claim.Value, out Guid guidValue)) return Result<Guid>.Failure(GetTokenFromRequestErrors.NotGuid);

            return Result<Guid>.Success(guidValue);
        }

        public static class GetTokenFromRequestErrors
        {
            public static readonly Error ClaimNotFound = Error.NotFound("GetTokenFromRequest.SerchingClaim", "The NameIdentifier claim was not found");
            public static readonly Error NotGuid = Error.Validation("GetTokenFromRequest.ReadingId", "The value of the NameIdentifier claim was not Guid");
        }
    }

}
