using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Project.DAL.DTOs.Input;
using Project.DAL.Entities;
using Project.DAL.Repositories.Generic;
using Project.DAL.Utils;

namespace Project.DAL.Repositories
{
    public class UserRepository(ProjectDbContext ctx, ILogger<UserRepository> logger) : GenericRepository<User>(ctx, logger), IUserRepository
    {
        PasswordHasher<User> passwordHasher = new();

        public List<User> GetAllByRole(Role role)
        {
            List<User>? users = Get(x => x.Active, "Roles").Result.Payload;
            return users.IsNullOrEmpty() ? [] : users!.FindAll(x => x.Roles.Contains(role));
        }

        public bool HasRole(Guid userId, Role role)
        {
            User? user = Get(x => x.Active && x.Id == userId, "Roles").Result.Payload?.FirstOrDefault();
            if (user == null) throw new Exception();
            return user.Roles.Contains(role);
        }

        public Result<User> Create(Register model)
        {

            // check the existance with the same username
            List<User>? userExists = Get(x => x.Active && x.Username == model.Username).Result.Payload;
            if (!userExists.IsNullOrEmpty()) return Result<User>.Failure(UserRepositoryErrors.ExistingUsername);

            // check the existance with the same email
            userExists = Get(x => x.Active && x.Email == model.Email).Result.Payload;
            if (!userExists.IsNullOrEmpty()) return Result<User>.Failure(UserRepositoryErrors.ExistingUsername);

            User user = new()
            {
                Email = model.Email,
                Username = model.Username,
                PasswordHash = ""
            };

            return Create(new User
            {
                Email = model.Email,
                Username = model.Username,
                PasswordHash = passwordHasher.HashPassword(user, model.Password),
            }).Result;
        }

        public static class UserRepositoryErrors
        {
            public static readonly Error ExistingUsername = Error.Conflict("UserRepository.Create", "Username already exists");
            public static readonly Error ExistingEmail = Error.Conflict("UserRepository.Create", "Email already exists");

        }
    }
}