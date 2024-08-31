using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Project.DAL.DTOs;
using Project.DAL.DTOs.Input;
using Project.DAL.DTOs.Output;
using Project.DAL.Entities;
using Project.DAL.Jwt;
using Project.DAL.Permit;
using Project.DAL.Repositories;
using Project.DAL.Utils;

namespace Project.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(
        IUnitOfWork repo,
        IJwtProvider jwtProvider,
        Mapper mapper,
        JwtOptions jwtOptions
        ) : ODataController
    {
        private readonly Mapper _mapper = mapper;
        private readonly IUnitOfWork _repo = repo;
        private readonly IJwtProvider _jwtProvider = jwtProvider;
        private readonly JwtOptions _jwtOptions = jwtOptions;


        [HttpGet]
        [Route("Test")]
        public IActionResult Get() => Ok("It works");

        //[Authorize]
        //[HasPermission(Permissions.ManageUsers)]
        [HttpGet]
        public IActionResult Get(ODataQueryOptions<User> options) => Ok(_repo.UserRepo.GetAllOData(options));

        [HttpGet]
        //[HasPermission(Permissions.ManageUsers)]
        [Route("GetSingleById")]
        public IResult GetSingleById(Guid id) => _repo.UserRepo.GetById(id).Result.ToHttpResult();

        //[Authorize]
        [HttpGet]
        [HasPermission(Permissions.ManageMyself)]
        [Route("SelfGet")]
        public IResult SelfGet()
        {
            Result<Guid> idResult = GetInfoFromToken.Id(HttpContext);
            if (idResult.IsFailure) return idResult.ToProblemDetails();

            Result<User?> userResult = _repo.UserRepo.GetById(idResult.Payload).Result;

            Result<MappedUser?> result = userResult.Payload is not null
                ? Result<MappedUser?>.Success(_mapper.MapUserToDTO(userResult.Payload))
                : Result<MappedUser?>.Failure(userResult.Error);

            return result.ToHttpResult();
        }

        [HttpPost]
        [Route("Login")]
        public IResult Login(Login model) => _repo.UserRepo.Login(model).Result.ToHttpResult();

        [HttpPost]
        [Route("RegisterUser")]
        public IResult RegisterUser(Register model) => _repo.UserRepo.Create(model).ToHttpResult();

        [HttpPost]
        [Route("RefreshToken")]
        public IResult RefreshToken(Tokens request) => _repo.UserRepo.RefreshToken(request).Result.ToHttpResult();

        /*
        [HttpPost]
        [HasPermission(Permissions.ManageUsers)]
        [Route("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdmin(Register model)
        {
            // check if audience exists
            if (_repo.UserRepo.Exist("Id", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!, user => !user.IsDeleted)) return StatusCode(StatusCodes.Status401Unauthorized, "Audience not found");

            // check the model's property
            if (string.IsNullOrEmpty(model.Password) || model.Password.Length > 40) return StatusCode(StatusCodes.Status400BadRequest, "Password is not valid");
            if (string.IsNullOrEmpty(model.Email) || model.Email.Length > 40 || !Regex.IsMatch(model.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")) return StatusCode(StatusCodes.Status400BadRequest, "Email is not valid");

            // check the existance with the same username
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null) return StatusCode(StatusCodes.Status400BadRequest, "Username already exists");

            // instantiate the object
            User user = new()
            {
                UserName = model.Username,
                Email = model.Email,
                IsDeleted = false,
                LockoutEnabled = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            // creating
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError, "Admin creation failed");

            // adding role
            _repo.UserRoleRepo.Create(new UserRole { UserId = user.Id, RoleId = Role.Admin.Id });

            return StatusCode(StatusCodes.Status201Created, user.Email);
        }

        [HttpPut]
        [HasPermission(Permissions.ManageUsers)]
        [Route("UserToAdmin")]
        public async Task<IActionResult> UserToAdmin(string id)
        {
            // check if audience exists
            if (_repo.UserRepo.Exist("Id", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!, user => !user.IsDeleted)) return StatusCode(StatusCodes.Status401Unauthorized, "Audience not found");

            // get user and check if exist
            User? user = await _repo.UserRepo.GetById(id);
            if (user == null) return StatusCode(StatusCodes.Status404NotFound, "User not found");
            if (user.IsDeleted) return StatusCode(StatusCodes.Status404NotFound, "User is deactivated");

            if (await _repo.UserRoleRepo.Create(new UserRole { UserId = user.Id, RoleId = Role.Admin.Id }))
            {
                return StatusCode(StatusCodes.Status200OK, "User upgraded successfully to admin");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "User upgrade to admin failed");
            }
        }

        [HttpPut]
        [HasPermission(Permissions.ManageUsers)]
        [Route("AdminToUser")]
        public async Task<IActionResult> AdminToUser(string id)
        {
            // check if audience exists
            if (_repo.UserRepo.Exist("Id", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!, user => !user.IsDeleted)) return StatusCode(StatusCodes.Status401Unauthorized, "Audience not found");

            // get user and check if exist
            User? user = await _repo.UserRepo.GetById(id);
            if (user == null) return StatusCode(StatusCodes.Status404NotFound, "User not found");
            if (user.IsDeleted) return StatusCode(StatusCodes.Status404NotFound, "User is deactivated");

            if (id == User.FindFirstValue(ClaimTypes.NameIdentifier)) return StatusCode(StatusCodes.Status400BadRequest, "You cannot downgrade yourself");

            _repo.UserRoleRepo.DeleteByFilter(x => x.UserId == user.Id && x.RoleId == Role.Admin.Id);

            return StatusCode(StatusCodes.Status200OK, "Admin downgraded to user successfully");
        }

        [HttpPut]
        [HasPermission(Permissions.ManageMyself)]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            // check if audience exists
            if (_repo.UserRepo.Exist("Id", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!, user => !user.IsDeleted)) return StatusCode(StatusCodes.Status401Unauthorized, "Audience not found");

            // get the user from the http context
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // checks on passwords
            if (model.CurrentPassword == model.NewPassword || await _userManager.CheckPasswordAsync(user!, model.CurrentPassword)) return StatusCode(StatusCodes.Status400BadRequest, "New and old password are equal");

            // update password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user!);
            var result = await _userManager.ResetPasswordAsync(user!, token, model.NewPassword);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, "Password updated successfully");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Password update failed");
            }
        }

        [HttpPut]
        [HasPermission(Permissions.ManageMyself)]
        [Route("ChangeUsername")]
        public async Task<IActionResult> ChangeUsername(ChangeUserName model)
        {

            // check if audience exists
            if (_repo.UserRepo.Exist("Id", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!, user => !user.IsDeleted)) return StatusCode(StatusCodes.Status401Unauthorized, "Audience not found");

            // checks on newUsername
            if (string.IsNullOrEmpty(model.NewUserName) || model.NewUserName.Length > 40 || model.NewUserName.Contains(" ")) return StatusCode(StatusCodes.Status400BadRequest, "Username is not valid");

            // check the existance with the same username
            var userExists = await _userManager.FindByNameAsync(model.NewUserName);
            if (userExists != null) return StatusCode(StatusCodes.Status400BadRequest, "Username already exists");

            // get the audience and check if exists
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (user == null) return StatusCode(StatusCodes.Status404NotFound, "User not found");
            if (user.IsDeleted) return StatusCode(StatusCodes.Status404NotFound, "User is deactivated");

            // checks on passwords
            if (await _userManager.CheckPasswordAsync(user!, model.CurrentPassword)) return StatusCode(StatusCodes.Status400BadRequest, "Password not valid");

            // update Username
            user.UserName = model.NewUserName;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, "Username updated successfully");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Username update failed");
            }
        }

        [HttpPut]
        [HasPermission(Permissions.ManageMyself)]
        [Route("ChangeEmail")]
        public async Task<IActionResult> ChangeEmail(ChangeEmail model)
        {
            // check if audience exists
            if (_repo.UserRepo.Exist("Id", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!, user => !user.IsDeleted)) return StatusCode(StatusCodes.Status401Unauthorized, "Audience not found");

            // checks on newEmail
            if (string.IsNullOrEmpty(model.NewEmail) || model.NewEmail.Length > 40 || !Regex.IsMatch(model.NewEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")) return StatusCode(StatusCodes.Status400BadRequest, "Email is not valid");

            // check the existance with the same email
            var userExists = await _userManager.FindByEmailAsync(model.NewEmail);
            if (userExists != null) return StatusCode(StatusCodes.Status400BadRequest, "Email already exists");

            // get the user from the http context
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // checks on passwords
            if (await _userManager.CheckPasswordAsync(user!, model.CurrentPassword)) return StatusCode(StatusCodes.Status400BadRequest, "Password not valid");

            // update Email
            user!.Email = model.NewEmail;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, "Email updated successfully");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Email update failed");
            }
        }

        [HttpDelete]
        [HasPermission(Permissions.ManageMyself)]
        [Route("SelfDelete")]
        public async Task<IActionResult> SelfDelete()
        {
            // check if audience exists
            if (_repo.UserRepo.Exist("Id", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!, user => !user.IsDeleted)) return StatusCode(StatusCodes.Status401Unauthorized, "Audience not found");

            // get the user from the http context
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // check the number of admin
            var roles = await _userManager.GetRolesAsync(user!);
            if (roles.Contains("Admin"))
            {
                List<UserRole> userRoles = _repo.UserRoleRepo.GetByFilter(x => x.RoleId == Role.Admin.Id);
                List<User> admins = _repo.UserRepo.GetAllByRole(Role.Admin);
                foreach (User item in admins) if (item.IsDeleted) admins.Remove(item);
                if (admins.Count <= 1) return StatusCode(StatusCodes.Status400BadRequest, "There is only one admin");
            }

            user!.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, "Entity deleted successfully");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Entity deletion failed");
            }
        }

        [HttpDelete]
        [HasPermission(Permissions.ManageUsers)]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            // check if audience exists
            if (_repo.UserRepo.Exist("Id", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!, user => !user.IsDeleted)) return StatusCode(StatusCodes.Status401Unauthorized, "Audience not found");

            // get user and check if exist
            User? user = await _repo.UserRepo.GetById(id);
            if (user == null) return StatusCode(StatusCodes.Status404NotFound, "User not found");
            if (user.IsDeleted) return StatusCode(StatusCodes.Status404NotFound, "User is deactivated");

            // check if admin
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin")) return StatusCode(StatusCodes.Status400BadRequest, "You cannot delete an admin.");

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, "Entity deleted successfully");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Entity deletion failed");
            }
        }

        [HttpDelete]
        [HasPermission(Permissions.ManageUsers)]
        [Route("Restore")]
        public async Task<IActionResult> Restore(string id)
        {
            // check if audience exists
            if (_repo.UserRepo.Exist("Id", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!, user => !user.IsDeleted)) return StatusCode(StatusCodes.Status401Unauthorized, "Audience not found");

            // get user and check if exist
            User? user = await _repo.UserRepo.GetById(id);
            if (user == null) return StatusCode(StatusCodes.Status404NotFound, "User not found");

            user.IsDeleted = false;
            var result = await _userManager.UpdateAsync(user);


            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, "Entity restored successfully");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Entity restoration failed");
            }
        }*/
    }
}