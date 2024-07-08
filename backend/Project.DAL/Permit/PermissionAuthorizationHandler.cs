using Microsoft.AspNetCore.Authorization;
using Project.DAL.Jwt;

namespace Project.DAL.Permit
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        public PermissionAuthorizationHandler()
        {
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {

            HashSet<string> permissions = context.User.Claims
                .Where(x => x.Type == CustomClaims.Permissions)
                .Select(x => x.Value)
                .ToHashSet();

            if (permissions.Contains(requirement.Permission)) context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}