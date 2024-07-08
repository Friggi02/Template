using Project.DAL.Repositories.Permission;

namespace Project.DAL.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepo { get; }
        IUserRoleRepository UserRoleRepo { get; }
        IPermissionRepository PermissionRepo { get; }
    }
}