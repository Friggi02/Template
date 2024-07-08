using Microsoft.EntityFrameworkCore;
using Project.DAL.Entities;
using System.Data;

namespace Project.DAL.Repositories.Permission
{
    public class PermissionRepository(ProjectDbContext ctx) : IPermissionRepository
    {
        protected readonly ProjectDbContext _ctx = ctx;
        public async Task<HashSet<string>> GetPermissionsAsync(Guid userId)
        {
            ICollection<Role>[] roles = await _ctx.Set<User>()
                .Include(x => x.Roles)
                .ThenInclude(x => x.Permissions)
                .Where(x => x.Id == userId)
                .Select(x => x.Roles)
                .ToArrayAsync();

            return roles
                .SelectMany(x => x)
                .SelectMany(x => x.Permissions)
                .Select(x => x.Name)
                .ToHashSet();
        }
    }
}