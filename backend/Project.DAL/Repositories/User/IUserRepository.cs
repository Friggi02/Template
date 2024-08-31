using Microsoft.AspNetCore.Http;
using Project.DAL.DTOs.Input;
using Project.DAL.DTOs.Output;
using Project.DAL.Entities;
using Project.DAL.Repositories.Generic;
using Project.DAL.Utils;
using System.Security.Claims;

namespace Project.DAL.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public List<User> GetAllByRole(Role role);
        public bool HasRole(Guid userId, Role role);
        public Result<User> Create(Register model);
        public Task<Result<LoginReturn>> Login(Login model);
        public Task<Result<string>> RefreshToken(Tokens request);
    }
}