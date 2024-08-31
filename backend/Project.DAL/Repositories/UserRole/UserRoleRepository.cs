using Microsoft.EntityFrameworkCore;
using Project.DAL.Entities;
using Project.DAL.Utils;

namespace Project.DAL.Repositories
{
    public class UserRoleRepository(ProjectDbContext ctx) : IUserRoleRepository
    {
        protected readonly ProjectDbContext _ctx = ctx;
        public Task<Result> Update(UserRole entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> Create(UserRole entity)
        {
            if (_ctx.Set<UserRole>().Where(x => x.UserId == entity.UserId && x.RoleId == entity.RoleId).FirstOrDefault() != null) return Result.Failure(UserRoleRepositoryErrors.Conflicts);

            await _ctx.AddAsync(entity);
            int saveResult = await _ctx.SaveChangesAsync();

            if (saveResult > 0)
                return Result.Success();
            else
                return Result.Failure(UserRoleRepositoryErrors.SaveChanges);
        }

        public async Task<Result<Role[]>> GetUserRoles(Guid userId)
        {
            var userRoles = await _ctx.Set<UserRole>()
                                      .Where(x => x.UserId == userId)
                                      .Select(x => x.RoleId)
                                      .ToListAsync();

            var roles = await _ctx.Set<Role>()
                                  .Where(x => userRoles.Contains(x.Id))
                                  .ToArrayAsync();

            return Result<Role[]>.Success(roles);
        }
    }

    public static class UserRoleRepositoryErrors
    {
        public static readonly Error Conflicts = Error.Conflict("UserRoleRepository.Confilcts", "The UserRole provided already exists");
        public static readonly Error SaveChanges = Error.Failure("UserRoleRepository.SaveChanges", "SaveChanges function returned 0");
    }
}