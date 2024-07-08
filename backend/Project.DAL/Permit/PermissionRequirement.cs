using Microsoft.AspNetCore.Authorization;

namespace Project.DAL.Permit
{
    public class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}