using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Project.DAL.DTOs;
using Project.DAL.DTOs.Input;
using Project.DAL.DTOs.Output;
using Project.DAL.Entities;
using Project.DAL.Jwt;
using Project.DAL.Repositories.Generic;
using Project.DAL.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Project.DAL.Repositories
{
    public class UserRepository(
        ProjectDbContext ctx,
        ILogger<UserRepository> logger,
        IConfiguration configuration,
        Mapper mapper,
        IJwtProvider jwtProvider
     ) : GenericRepository<User>(ctx, logger), IUserRepository
    {
        private readonly Mapper _mapper = mapper;
        PasswordHasher<User> passwordHasher = new();
        public List<User> GetAllByRole(Role role)
        {
            List<User>? users = Get(x => x.Active, "Roles").Result.Payload;
            return users.IsNullOrEmpty() ? [] : users!.FindAll(x => x.Roles.Contains(role));
        }

        public bool HasRole(Guid userId, Role role)
        {
            User? user = Get(x => x.Active && x.Id == userId, "Roles").Result.Payload?.FirstOrDefault();
            return user == null ? throw new Exception() : user.Roles.Contains(role);
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

        public async Task<Result<LoginReturn>> Login(Login model)
        {
            // getting the accessFailedMax from appsetting.json
            int accessFailedMax;
            try
            {
                accessFailedMax = configuration.GetValue<int>("SecuritySettings:AccessFailedMax");
            }
            catch (Exception) { return Result<LoginReturn>.Failure(UserRepositoryErrors.AccessFailedMax); }

            // getting the lockoutTime from appsetting.json
            TimeSpan lockoutTime;
            try
            {
                lockoutTime = configuration.GetValue<TimeSpan>("SecuritySettings:LockoutTime");
            }
            catch (Exception) { return Result<LoginReturn>.Failure(UserRepositoryErrors.LockoutTime); }

            User? user;
            Result result;

            if (model.EmailOrUsername.Contains('@'))
            {
                user = Get(x => x.Email == model.EmailOrUsername, "Roles").Result.Payload?.FirstOrDefault();
            }
            else
            {
                user = Get(x => x.Username == model.EmailOrUsername, "Roles").Result.Payload?.FirstOrDefault();
            }

            if (user is null) return Result<LoginReturn>.Failure(UserRepositoryErrors.NotFoundLogin);
            if (!user.Active) return Result<LoginReturn>.Failure(UserRepositoryErrors.Deactivated);
            if (user.IsLockedout()) return Result<LoginReturn>.Failure(UserRepositoryErrors.LockedOut(user.LockoutEnd));
            
            // check the number of accesses failed and set lockout
            if (user.AccessFailedCount > accessFailedMax)
            {
                user.AccessFailedCount = 0;
                user.LockoutEnd = DateTime.UtcNow + lockoutTime;
                result = await Update(user);
                if (result.IsFailure) return Result<LoginReturn>.Failure(result.Error);
                return Result<LoginReturn>.Failure(UserRepositoryErrors.LockedOut(user.LockoutEnd));
            }

            // check if the password is correct
            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
            {
                user.AccessFailedCount++;
                result = await Update(user);
                if (result.IsFailure) return Result<LoginReturn>.Failure(result.Error);
                return Result<LoginReturn>.Failure(UserRepositoryErrors.AccessDenied);
            }

            // remove the lockout if and reset AccessFailedCount
            if (user.AccessFailedCount != 0 || user.LockoutEnd is not null)
            {
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                result = await Update(user);
                if (result.IsFailure) return Result<LoginReturn>.Failure(result.Error);
            }

            // build tokens
            string newAccessToken = await jwtProvider.GenerateAccessToken(user);
            string newRefreshToken = jwtProvider.GenerateRefreshToken(user);

            // saving the refresh token
            user.RefreshToken = newRefreshToken;
            result = await Update(user);
            if (result.IsFailure) return Result<LoginReturn>.Failure(result.Error);

            return Result<LoginReturn>.Success(new LoginReturn
            {
                Tokens = new Tokens
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                },
                User = _mapper.MapUserToDTO(user)
            });
        }

        public async Task<Result<string>>RefreshToken(Tokens request)
        {
            Result result;
            Result<Guid> id = GetInfoFromToken.Id(request.AccessToken);

            if (id.IsFailure) return Result<string>.Failure(UserRepositoryErrors.NotFoundRefreshToken);

            User? user = Get(x => x.Id == id.Payload).Result.Payload?.FirstOrDefault();
            if (user is null) return Result<string>.Failure(UserRepositoryErrors.NotFoundRefreshToken);
            if (!user.Active) return Result<string>.Failure(UserRepositoryErrors.Deactivated);
            if (user.IsLockedout()) return Result<string>.Failure(UserRepositoryErrors.LockedOut(user.LockoutEnd));


            // check the refresh token on the db
            if (user.RefreshToken != request.RefreshToken) return Result<string>.Failure(UserRepositoryErrors.RefreshTokenNotValid);

            // check the token's expire date

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(request.RefreshToken);
            DateTime expiryDate = jwtSecurityToken.ValidTo;

            if (new JwtSecurityTokenHandler().ReadToken(request.RefreshToken) is not JwtSecurityToken jsonToken) return Result<string>.Failure(UserRepositoryErrors.RefreshTokenNotValid);
            if (expiryDate < DateTime.UtcNow)
            {
                user.RefreshToken = null;
                result = await Update(user);
                if (result.IsFailure) return Result<string>.Failure(result.Error);
            }

            // build tokens
            string newAccessToken = await jwtProvider.GenerateAccessToken(user);

            return Result<string>.Success(newAccessToken);
        }
        public static class UserRepositoryErrors
        {
            public static readonly Error AccessFailedMax = Error.Conflict("UserRepository.ReadingAppsettings", "Failed reading AccessFailedMax from appsetting.json");
            public static readonly Error LockoutTime = Error.Conflict("UserRepository.ReadingAppsettings", "Failed reading LockoutTime from appsetting.json");
            public static readonly Error ExistingUsername = Error.Conflict("UserRepository.Create", "Username already exists");
            public static readonly Error ExistingEmail = Error.Conflict("UserRepository.Create", "Email already exists");
            public static readonly Error NotFoundLogin = Error.Validation("UserRepository.Login", "User not found");
            public static readonly Error Deactivated = Error.Validation("UserRepository.Login", "User deactivated");
            public static readonly Error AccessDenied = Error.Validation("UserRepository.Login", "Access denied");
            public static readonly Error NotFoundRefreshToken = Error.Validation("UserRepository.RefreshToken", "User not found");
            public static readonly Error RefreshTokenNotValid = Error.Validation("UserRepository.RefreshToken", "RefreshToken not valid");
            public static readonly Error RefreshTokenExpired = Error.Validation("UserRepository.RefreshToken", "RefreshToken expired");
            public static Error LockedOut(DateTime? until) => Error.Validation("UserRepository.Login", $"Too many tries. You're locked out until {until}");
        }
    }
}