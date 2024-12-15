using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Project.DAL.DTOs;
using Project.DAL.DTOs.Input;
using Project.DAL.DTOs.Output;
using Project.DAL.Entities;
using Project.DAL.Permit;
using Project.DAL.Repositories;
using Project.DAL.Utils;

namespace Project.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(
        IUnitOfWork repo
    ) : ODataController
    {
        private readonly IUnitOfWork _repo = repo;

        [HttpGet]
        [HasPermission(Permissions.ManageUsers)]
        public IActionResult Get(ODataQueryOptions<User> options) => Ok(_repo.UserRepo.GetAllOData(options));

        [HttpGet]
        [Route("Test")]
        public IActionResult Test() => Ok("It works");

        [HttpGet]
        [HasPermission(Permissions.ManageUsers)]
        [Route("GetSingleById")]
        public IResult GetSingleById(Guid id) => _repo.UserRepo.GetById(id).Result.ToHttpResult();

        [HttpGet]
        [HasPermission(Permissions.ManageMyself)]
        [Route("SelfGet")]
        public IResult SelfGet() => _repo.UserRepo.SelfGet(HttpContext).ToHttpResult();

        [HttpPost]
        [Route("Login")]
        public IResult Login(Login model) => _repo.UserRepo.Login(model).Result.ToHttpResult();

        [HttpPost]
        [Route("RegisterUser")]
        public IResult RegisterUser(Register model) => _repo.UserRepo.Create(model).ToHttpResult();

        [HttpPost]
        [Route("RefreshToken")]
        public IResult RefreshToken(Tokens request) => _repo.UserRepo.RefreshToken(request).Result.ToHttpResult();

        [HttpPut]
        [HasPermission(Permissions.ManageUsers)]
        [Route("AssignRole")]
        public IResult AssignRole(Guid userId, string role) => _repo.UserRepo.AssignRole(userId, role).ToHttpResult();

        [HttpPut]
        [HasPermission(Permissions.ManageUsers)]
        [Route("RevokeRole")]
        public IResult RevokeRole(Guid userId, string role) => _repo.UserRepo.RevokeRole(userId, role).ToHttpResult();

        [HttpPut]
        [HasPermission(Permissions.ManageMyself)]
        [Route("ChangePassword")]
        public IResult ChangePassword(ChangePassword model) => _repo.UserRepo.ChangePassword(model, HttpContext).Result.ToHttpResult();

        [HttpPut]
        [HasPermission(Permissions.ManageMyself)]
        [Route("ChangeUsername")]
        public IResult ChangeUsername(ChangeUsername model) => _repo.UserRepo.ChangeUsername(model, HttpContext).Result.ToHttpResult();

        [HttpPut]
        [HasPermission(Permissions.ManageMyself)]
        [Route("ChangeEmail")]
        public IResult ChangeEmail(ChangeEmail model) => _repo.UserRepo.ChangeEmail(model, HttpContext).Result.ToHttpResult();

        [HttpDelete]
        [HasPermission(Permissions.ManageMyself)]
        [Route("SelfDelete")]
        public IResult SelfDelete(SelfDelete model) => _repo.UserRepo.SelfDelete(model, HttpContext).Result.ToHttpResult();

        [HttpDelete]
        [HasPermission(Permissions.ManageUsers)]
        [Route("Delete")]
        public IResult Delete(Guid userId) => _repo.UserRepo.SoftDelete(userId).Result.ToHttpResult();

        [HttpDelete]
        [HasPermission(Permissions.ManageUsers)]
        [Route("Restore")]
        public IResult Restore(Guid userId) => _repo.UserRepo.Restore(userId).Result.ToHttpResult();
    }
}