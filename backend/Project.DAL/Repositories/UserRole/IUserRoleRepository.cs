using Project.DAL.Entities;
using Project.DAL.Utils;

namespace Project.DAL.Repositories
{
    public interface IUserRoleRepository
    {
        public Task<Result> Create(UserRole entity);
    }
}