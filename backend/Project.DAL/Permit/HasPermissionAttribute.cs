using Microsoft.AspNetCore.Authorization;

namespace Project.DAL.Permit
{
    public sealed class HasPermissionAttribute(Permissions permission) : AuthorizeAttribute(policy: permission.ToString())
    {
    }
}