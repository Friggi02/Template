using Project.DAL.DTOs.Input;
using Project.DAL.Entities;
using Project.DAL.Repositories.Generic;
using Project.DAL.Utils;

namespace Project.DAL.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public List<User> GetAllByRole(Role role);
        public bool HasRole(Guid userId, Role role);
        public Result<User> Create(Register model);
    }
}