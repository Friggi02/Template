﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Project.DAL.Jwt;
using Project.DAL.Repositories.Permission;

namespace Project.DAL.Repositories.UnitOfWork
{
    public class UnitOfWork(ProjectDbContext ctx, ILogger<UserRepository> logger, IConfiguration configuration, IJwtProvider jwtProvider) : IUnitOfWork
    {
        public IUserRepository UserRepo { get; private set; } = new UserRepository(ctx, logger, configuration, jwtProvider);
        public IUserRoleRepository UserRoleRepo { get; private set; } = new UserRoleRepository(ctx);
        public IPermissionRepository PermissionRepo { get; private set; } = new PermissionRepository(ctx);
    }
}