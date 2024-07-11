using Microsoft.AspNetCore.OData.Query;
using Project.DAL.Utils;
using System.Linq.Expressions;

namespace Project.DAL.Repositories.Generic
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        #region Get

        /// <summary>
        /// Retrieves an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
        Task<Result<T?>> GetById(Guid id);

        /// <summary>
        /// Retrieves a list of entities that satisfy the specified predicate and includes the specified navigation property for eager loading.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition. Null by default.</param>
        /// <param name="navigationProperty">The navigation property to include. Null by default.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities that satisfy the condition.</returns>
        Task<Result<List<T>>> Get(Expression<Func<T, bool>>? predicate = null, string? navigationProperty = null);

        /// <summary>
        /// Applies the specified OData query options to the entities and returns the result.
        /// </summary>
        /// <param name="options">The OData query options to apply.</param>
        /// <returns>The result of applying the query options to the entities.</returns>
        Result<IQueryable> GetAllOData(ODataQueryOptions<T> options);

        #endregion

        #region Create

        /// <summary>
        /// Creates a new entity in the database.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created entity if the operation was successful; otherwise, an error message.</returns>
        Task<Result<T>> Create(T entity);

        #endregion

        #region Delete

        /// <summary>
        /// Deletes an entity from the database by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<Result> HardDelete(Guid id);

        /// <summary>
        /// Deletes entities from the database that satisfy the specified predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<Result> HardDeleteByFilter(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Soft deletes an entity from the database by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<Result> SoftDelete(Guid id);

        /// <summary>
        /// Soft deletes entities from the database that satisfy the specified predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<Result> SoftDeleteByFilter(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Restores a soft deleted entity in the database by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<Result> Restore(Guid id);

        /// <summary>
        /// Restores soft deleted entities in the database that satisfy the specified predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<Result> RestoreByFilter(Expression<Func<T, bool>> predicate);

        #endregion

        #region Exist

        /// <summary>
        /// Checks if any entity satisfies the specified predicate.
        /// </summary>
        /// <param name="predicate">A function to test each entity for a condition.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether any entity satisfies the condition.</returns>
        Task<Result<bool>> Exist(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Checks if an entity exists by its id and an optional predicate.
        /// </summary>
        /// <param name="id">The id of the entity to check.</param>
        /// <param name="predicate">An optional function to test each entity for a condition.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the entity exists.</returns>
        Task<Result<bool>> ExistById(Guid id, Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Checks if an active entity exists by its id and an optional predicate.
        /// </summary>
        /// <param name="id">The id of the entity to check.</param>
        /// <param name="predicate">An optional function to test each entity for a condition.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the active entity exists.</returns>
        Task<Result<bool>> ExistActive(Guid id, Expression<Func<T, bool>>? predicate = null);

        #endregion

        /// <summary>
        /// Applies the specified patch document to the current entity.
        /// </summary>
        /// <param name="patch">The patch document to apply.</param>
        /// <param name="current">The current entity to apply the patch to.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<Result> Patch(Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<T> patch, T current);

        /// <summary>
        /// Update the entity baesd on Id with the values of the current entity.
        /// </summary>
        /// <param name="entity">The entity with the new values.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        public Task<Result> Update(T entity);
    }
}