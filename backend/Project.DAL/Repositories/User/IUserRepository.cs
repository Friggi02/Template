using Microsoft.AspNetCore.Http;
using Project.DAL.DTOs.Input;
using Project.DAL.DTOs.Output;
using Project.DAL.Entities;
using Project.DAL.Repositories.Generic;
using Project.DAL.Utils;

namespace Project.DAL.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public List<User> GetAllByRole(Role role);
        public bool HasRole(Guid userId, Role role);
        public Result<User?> GetFromHttpContext(HttpContext httpContext);
        public Result<User?> SelfGet(HttpContext httpContext);
        public Result AssignRole(Guid userId, string role);
        public Result RevokeRole(Guid userId, string role);
        public Result<User> Create(Register model);
        public Task<Result<LoginReturn>> Login(Login model);
        public Task<Result<string>> RefreshToken(Tokens request);
        public Task<Result> ChangePassword(ChangePassword model, HttpContext httpContext);
        public Task<Result> ChangeUsername(ChangeUsername model, HttpContext httpContext);
        public Task<Result> ChangeEmail(ChangeEmail model, HttpContext httpContext);
        public Task<Result> SelfDelete(SelfDelete model, HttpContext httpContext);
    }
}