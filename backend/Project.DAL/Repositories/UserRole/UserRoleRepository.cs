using Microsoft.EntityFrameworkCore;
using Project.DAL.Entities;
using Project.DAL.Utils;

namespace Project.DAL.Repositories
{
    public class UserRoleRepository(ProjectDbContext ctx) : IUserRoleRepository
    {
        protected readonly ProjectDbContext _ctx = ctx;

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

        public async Task<Result> Delete(UserRole entity)
        {
            if (_ctx.Set<UserRole>().Where(x => x.UserId == entity.UserId && x.RoleId == entity.RoleId).FirstOrDefault() == null) return Result.Failure(UserRoleRepositoryErrors.NotFound);

            _ctx.Remove(entity);
            int saveResult = await _ctx.SaveChangesAsync();

            if (saveResult > 0)
                return Result.Success();
            else
                return Result.Failure(UserRoleRepositoryErrors.SaveChanges);
        }

        public async Task<Result> DeleteByRole(Guid roleId)
        {
            IQueryable<UserRole> entities = _ctx.Set<UserRole>().Where(x => x.RoleId == roleId);
            _ctx.RemoveRange(entities);
            int saveResult = await _ctx.SaveChangesAsync();

            if (saveResult > 0)
                return Result.Success();
            else
                return Result.Failure(UserRoleRepositoryErrors.SaveChanges);
        }

        public async Task<Result> DeleteByUser(Guid userId)
        {
            IQueryable<UserRole> entities = _ctx.Set<UserRole>().Where(x => x.UserId == userId);
            _ctx.RemoveRange(entities);
            int saveResult = await _ctx.SaveChangesAsync();

            if (saveResult > 0)
                return Result.Success();
            else
                return Result.Failure(UserRoleRepositoryErrors.SaveChanges);
        }
    }

    public static class UserRoleRepositoryErrors
    {
        public static readonly Error Conflicts = Error.Conflict("UserRoleRepository.Create", "The UserRole provided already exists");
        public static readonly Error NotFound = Error.Conflict("UserRoleRepository.Delete", "UserRole provided not found");
        public static readonly Error SaveChanges = Error.Failure("UserRoleRepository", "SaveChanges function returned 0");
    }
}