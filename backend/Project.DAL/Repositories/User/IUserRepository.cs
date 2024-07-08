using Project.DAL.Entities;
using Project.DAL.Repositories.Generic;

namespace Project.DAL.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public List<User> GetAllByRole(Role role);
        public bool HasRole(Guid userId, Role role);
    }
}