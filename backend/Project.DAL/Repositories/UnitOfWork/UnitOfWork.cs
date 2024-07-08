using Microsoft.Extensions.Logging;
using Project.DAL.Repositories.Permission;

namespace Project.DAL.Repositories.UnitOfWork
{
    public class UnitOfWork(ProjectDbContext ctx, ILogger<UserRepository> logger) : IUnitOfWork
    {
        public IUserRepository UserRepo { get; private set; } = new UserRepository(ctx, logger);
        public IUserRoleRepository UserRoleRepo { get; private set; } = new UserRoleRepository(ctx);
        public IPermissionRepository PermissionRepo { get; private set; } = new PermissionRepository(ctx);
    }
}