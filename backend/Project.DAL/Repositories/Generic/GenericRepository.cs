using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Project.DAL.Utils;
using System.Linq.Expressions;
using System.Reflection;

namespace Project.DAL.Repositories.Generic
{
    public class GenericRepository<T>(ProjectDbContext ctx, ILogger<GenericRepository<T>> logger) where T : BaseEntity
    {
        protected readonly ProjectDbContext _ctx = ctx;
        private readonly ILogger _logger = logger;
        internal DbSet<T> DbSet { get; set; } = ctx.Set<T>();

        #region Get

        public async Task<Result<T?>> GetById(Guid id)
        {
            _logger.LogInformation($"Executing GetById - id = {id}");

            try
            {
                T? entity = await DbSet.FindAsync(id);
                return Result<T?>.Success(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing GetById: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result<T?>.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public async Task<Result<List<T>>> Get(Expression<Func<T, bool>>? predicate = null, string? navigationProperty = null)
        {
            try
            {
                IQueryable<T> queryable = DbSet.AsQueryable();

                if (predicate != null)
                {
                    _logger.LogInformation($"Executing GetByFilter");
                    queryable = queryable.Where(predicate);
                }

                if (navigationProperty != null)
                {
                    _logger.LogInformation($"Executing GetAllIncluding - navigationProperty = {navigationProperty}");
                    queryable = queryable.Include(navigationProperty);
                }

                if (predicate == null && navigationProperty == null)
                {
                    _logger.LogInformation($"Executing GetAll");
                }

                List<T> entities = await queryable.ToListAsync();
                return Result<List<T>>.Success(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing Get: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result<List<T>>.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public Result<IQueryable> GetAllOData(ODataQueryOptions<T> options)
        {
            _logger.LogInformation($"Executing GetAllOData");

            try
            {
                IQueryable queryable = options.ApplyTo(DbSet.AsQueryable());
                return Result<IQueryable>.Success(queryable);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing GetAllOData: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result<IQueryable>.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        #endregion

        #region Create

        public async Task<Result<T>> Create(T entity)
        {

            _logger.LogInformation($"Executing Create");

            try
            {
                await _ctx.AddAsync(entity);
                int saveResult = await _ctx.SaveChangesAsync();

                return saveResult > 0 ? Result<T>.Success(entity) : Result<T>.Failure(GenericRepositoryErrors.SaveChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing Create: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result<T>.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        #endregion

        #region Delete

        public async Task<Result> HardDelete(Guid id)
        {
            _logger.LogInformation($"Executing HardDelete - id = {id}");

            try
            {
                Result<T?> entityResult = await GetById(id);
                if (entityResult.IsFailure || entityResult.Payload is null) return Result.Failure(GenericRepositoryErrors.NotFound(id));

                _ctx.Remove(entityResult.Payload);

                int saveResult = await _ctx.SaveChangesAsync();
                return saveResult > 0 ? Result.Success() : Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing HardDelete: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public async Task<Result> HardDeleteByFilter(Expression<Func<T, bool>> predicate)
        {
            _logger.LogInformation($"Executing HardDeleteByFilter");

            try
            {
                Result<List<T>> entitiesResult = await Get(predicate);
                if (entitiesResult.IsFailure || entitiesResult.Payload.IsNullOrEmpty()) return Result.Failure(GenericRepositoryErrors.NotFoundRange);

                _ctx.RemoveRange(entitiesResult.Payload!);

                int saveResult = await _ctx.SaveChangesAsync();
                return saveResult > 0 ? Result.Success() : Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing HardDeleteByFilter: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public async Task<Result> SoftDelete(Guid id)
        {
            _logger.LogInformation($"Executing SoftDelete - id = {id}");

            try
            {
                Result<T?> entityResult = await GetById(id);
                if (entityResult.IsFailure || entityResult.Payload is null) return Result.Failure(GenericRepositoryErrors.NotFound(id));

                entityResult.Payload.Active = false;

                int saveResult = await _ctx.SaveChangesAsync();
                return saveResult > 0 ? Result.Success() : Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing SoftDelete: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public async Task<Result> SoftDeleteByFilter(Expression<Func<T, bool>> predicate)
        {
            _logger.LogInformation($"Executing SoftDeleteByFilter");

            try
            {
                Result<List<T>> entitiesResult = await Get(predicate);
                if (entitiesResult.IsFailure || entitiesResult.Payload.IsNullOrEmpty()) return Result.Failure(GenericRepositoryErrors.NotFoundRange);

                foreach (T item in entitiesResult.Payload!)
                {
                    item.Active = false;
                }

                int saveResult = await _ctx.SaveChangesAsync();
                return saveResult > 0 ? Result.Success() : Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing SoftDeleteByFilter: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public async Task<Result> Restore(Guid id)
        {
            _logger.LogInformation($"Executing Restore - id = {id}");

            try
            {
                Result<T?> entityResult = await GetById(id);
                if (entityResult.IsFailure || entityResult.Payload is null) return Result.Failure(GenericRepositoryErrors.NotFound(id));

                entityResult.Payload.Active = true;

                int saveResult = await _ctx.SaveChangesAsync();
                return saveResult > 0 ? Result.Success() : Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing Restore: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public async Task<Result> RestoreByFilter(Expression<Func<T, bool>> predicate)
        {
            _logger.LogInformation($"Executing RestoreByFilter");

            try
            {
                Result<List<T>> entitiesResult = await Get(predicate);
                if (entitiesResult.IsFailure || entitiesResult.Payload.IsNullOrEmpty()) return Result.Failure(GenericRepositoryErrors.NotFoundRange);

                foreach (T item in entitiesResult.Payload!)
                {
                    item.Active = true;
                }

                int saveResult = await _ctx.SaveChangesAsync();
                return saveResult > 0 ? Result.Success() : Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing RestoreByFilter: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        #endregion

        #region Exist

        public async Task<Result<bool>> Exist(Expression<Func<T, bool>> predicate)
        {
            _logger.LogInformation($"Executing Exist");

            try
            {
                bool exist = await DbSet.Where(predicate).AnyAsync();
                return Result<bool>.Success(exist);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing Exist: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result<bool>.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public async Task<Result<bool>> ExistById(Guid id, Expression<Func<T, bool>>? predicate = null)
        {
            _logger.LogInformation($"Executing ExistById - id = {id}");

            try
            {
                bool exist = predicate == null
                    ? await DbSet.AnyAsync(x => x.Id == id)
                    : await DbSet.Where(predicate).AnyAsync(x => x.Id == id);
                return Result<bool>.Success(exist);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing ExistById: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result<bool>.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public async Task<Result<bool>> ExistActive(Guid id, Expression<Func<T, bool>>? predicate = null)
        {
            _logger.LogInformation($"Executing ExistActive - id = {id}");

            try
            {
                bool exist = predicate == null
                    ? await DbSet.AnyAsync(x => x.Id == id && x.Active)
                    : await DbSet.Where(predicate).AnyAsync(x => x.Id == id && x.Active);
                return Result<bool>.Success(exist);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing ExistActive: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result<bool>.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        #endregion

        public async Task<Result> Patch(Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<T> patch, T current)
        {

            _logger.LogInformation($"Executing Patch");

            try
            {
                patch.ApplyTo(current);

                if (await _ctx.SaveChangesAsync() > 0)
                    return Result.Success();
                else
                    return Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing Patch: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public async Task<Result> Update(T entity)
        {
            _logger.LogInformation($"Executing Update");

            try
            {
                Result<T?> entityResult = await GetById(entity.Id);
                if (entityResult.IsFailure || entityResult.Payload is null) return Result.Failure(GenericRepositoryErrors.NotFound(entity.Id));

                foreach (PropertyInfo inputProperty in typeof(T).GetProperties())
                {
                    PropertyInfo? outputProperty = typeof(T).GetProperty(inputProperty.Name);
                    if (outputProperty != null && inputProperty.PropertyType == outputProperty.PropertyType)
                    {
                        outputProperty.SetValue(entityResult.Payload, inputProperty.GetValue(entity));
                    }
                }

                int saveResult = await _ctx.SaveChangesAsync();
                return saveResult > 0 ? Result.Success() : Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while executing Update: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Result.Failure(GenericRepositoryErrors.SaveChanges);
            }
        }

        public static class GenericRepositoryErrors
        {
            public static readonly Error SaveChanges = Error.Failure("GenericRepository.SaveChanges", "SaveChanges function returned 0");
            public static Error NotFound<TKey>(TKey id) => Error.NotFound("GenericRepository.NotFound", $"The {nameof(T)} - {id} wasn't found in the database");
            public static readonly Error NotFoundRange = Error.NotFound("GenericRepository.NotFound", $"No {nameof(T)}s corresponding to the query found in the database");
        }
    }
}