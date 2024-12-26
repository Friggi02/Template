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
using System.Text.RegularExpressions;

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

        public Result<User?> GetFromHttpContext(HttpContext httpContext)
        {
            Guid? idResult = GetInfoFromToken.Id(httpContext);

            return idResult is not null
                ? GetById(idResult.Value).Result
                : Result<User?>.Failure(UserRepositoryErrors.ClaimNotFound);
        }
        
        public Result<User?> SelfGet(HttpContext httpContext)
        {
            return GetFromHttpContext(httpContext);
        }
        
        public Result AssignRole(Guid userId, string role)
        {
            throw new NotImplementedException();
        }

        public Result RevokeRole(Guid userId, string role)
        {
            throw new NotImplementedException();
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

        public async Task<Result<string>> RefreshToken(Tokens request)
        {
            Result result;
            Guid? id = GetInfoFromToken.Id(request.AccessToken);

            if (id is null) return Result<string>.Failure(UserRepositoryErrors.NotFoundRefreshToken);

            User? user = Get(x => x.Id == id).Result.Payload?.FirstOrDefault();
            if (user is null) return Result<string>.Failure(UserRepositoryErrors.NotFoundRefreshToken);
            if (!user.Active) return Result<string>.Failure(UserRepositoryErrors.Deactivated);
            if (user.IsLockedout()) return Result<string>.Failure(UserRepositoryErrors.LockedOut(user.LockoutEnd));

            // check the refresh token on the db
            if (user.RefreshToken != request.RefreshToken) return Result<string>.Failure(UserRepositoryErrors.RefreshTokenNotValid);

            // check the token's expire date

            JwtSecurityTokenHandler handler = new();
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

        public async Task<Result> ChangePassword(ChangePassword model, HttpContext httpContext)
        {
            // perform checks
            if (model.CurrentPassword == model.NewPassword) return Result.Failure(UserRepositoryErrors.PasswordsEqual);

            // retrive the user
            Result<User?> userResult = GetFromHttpContext(httpContext);
            if (userResult.IsFailure) return Result.Failure(userResult.Error);

            // check password
            if (passwordHasher.VerifyHashedPassword(userResult.Payload!, userResult.Payload!.PasswordHash, model.CurrentPassword) != PasswordVerificationResult.Success) return Result.Failure(UserRepositoryErrors.WrongPassword);

            // update password
            userResult.Payload.PasswordHash = passwordHasher.HashPassword(userResult.Payload, model.NewPassword);
            return await Update(userResult.Payload);
        }

        public async Task<Result> ChangeUsername(ChangeUsername model, HttpContext httpContext)
        {
            // checks on newUsername
            if (string.IsNullOrEmpty(model.NewUserName) || model.NewUserName.Length > 40 || model.NewUserName.Contains(" ")) return Result.Failure(UserRepositoryErrors.UsernameNotValid);

            // check the existance with the same username
            var userExists = await Exist(x => x.Username == model.NewUserName);
            if (userExists.IsFailure) return Result.Failure(userExists.Error);
            if (userExists.Payload) return Result.Failure(UserRepositoryErrors.UsernameAlreadyExists);

            // retrive the user
            Result<User?> userResult = GetFromHttpContext(httpContext);
            if (userResult.IsFailure) return Result.Failure(userResult.Error);

            // check password
            if (passwordHasher.VerifyHashedPassword(userResult.Payload!, userResult.Payload!.PasswordHash, model.CurrentPassword) != PasswordVerificationResult.Success) return Result.Failure(UserRepositoryErrors.WrongPassword);

            // update Username
            userResult.Payload.Username = model.NewUserName;
            return await Update(userResult.Payload);
        }

        public async Task<Result> ChangeEmail(ChangeEmail model, HttpContext httpContext)
        {
            // checks on newEmail
            if (string.IsNullOrEmpty(model.NewEmail) || model.NewEmail.Length > 40 || !Regex.IsMatch(model.NewEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")) return Result.Failure(UserRepositoryErrors.InvalidEmail);

            // check the existance with the same email
            var userExists = await Exist(x => x.Email == model.NewEmail);
            if (userExists.IsFailure) return Result.Failure(userExists.Error);
            if (userExists.Payload) return Result.Failure(UserRepositoryErrors.EmailAlreadyExists);

            // retrive the user
            Result<User?> userResult = GetFromHttpContext(httpContext);
            if (userResult.IsFailure) return Result.Failure(userResult.Error);

            // check password
            if (passwordHasher.VerifyHashedPassword(userResult.Payload!, userResult.Payload!.PasswordHash, model.CurrentPassword) != PasswordVerificationResult.Success) return Result.Failure(UserRepositoryErrors.WrongPassword);

            // update Email
            userResult.Payload.Email = model.NewEmail;
            return await Update(userResult.Payload);
        }

        public async Task<Result> SelfDelete(SelfDelete model, HttpContext httpContext)
        {
            // retrive the user
            Result<User?> userResult = GetFromHttpContext(httpContext);
            if (userResult.IsFailure) return Result.Failure(userResult.Error);

            // check if is the last admin
            if (userResult.Payload!.Roles.Contains(Role.Admin))
            {
                var admins = GetAllByRole(Role.Admin);
                admins.Remove(userResult.Payload);
                if (admins.Count == 0) return Result.Failure(UserRepositoryErrors.LastAdmin);
            }

            // check password
            if (passwordHasher.VerifyHashedPassword(userResult.Payload!, userResult.Payload!.PasswordHash, model.CurrentPassword) != PasswordVerificationResult.Success) return Result.Failure(UserRepositoryErrors.WrongPassword);

            // delete
            return await HardDelete(userResult.Payload.Id);
        }

        public static class UserRepositoryErrors
        {
            public static readonly Error ClaimNotFound = Error.NotFound("UserRepository.SerchingClaim", "The NameIdentifier claim was not found or it wasn't a GUID");
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
            public static readonly Error PasswordsEqual = Error.Validation("UserRepository.ChangePassword", "New and old password are equal");
            public static readonly Error UsernameNotValid = Error.Validation("UserRepository.ChangeUsername", "Username is not valid");
            public static readonly Error UsernameAlreadyExists = Error.Conflict("UserRepository.ChangeUsername", "Username already exists");
            public static readonly Error EmailAlreadyExists = Error.Conflict("UserRepository.ChangeEmail", "Email already exists");
            public static readonly Error WrongPassword = Error.Validation("UserRepository", "Invalid password");
            public static readonly Error InvalidEmail = Error.Validation("UserRepository.ChangeEmail", "Email is not valid");
            public static readonly Error LastAdmin = Error.Validation("UserRepository.SelfDelete", "You're the last admin");
            public static Error LockedOut(DateTime? until) => Error.Validation("UserRepository.Login", $"Too many tries. You're locked out until {until}");
        }
    }
}