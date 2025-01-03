﻿using Project.DAL.Entities;
using Project.DAL.Utils;

namespace Project.DAL.Repositories
{
    public interface IUserRoleRepository
    {
        public Task<Result> Create(UserRole entity);
        public Task<Result<Role[]>> GetUserRoles(Guid userId);
        public Task<Result> Delete(UserRole entity);
        public Task<Result> DeleteByRole(Guid roleId);
        public Task<Result> DeleteByUser(Guid userId);
    }
}