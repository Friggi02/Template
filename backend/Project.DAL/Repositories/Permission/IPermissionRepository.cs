namespace Project.DAL.Repositories.Permission
{
    public interface IPermissionRepository
    {
        public Task<HashSet<string>> GetPermissionsAsync(Guid userId);
    }
}