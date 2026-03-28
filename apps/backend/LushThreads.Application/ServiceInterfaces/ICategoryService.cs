using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Defines the contract for category-related business operations.
    /// </summary>
    public interface ICategoryService
    {
        #region Query Methods

        /// <summary>
        /// Retrieves all categories from the system.
        /// </summary>
        /// <returns>A collection of all <see cref="Category"/> entities.</returns>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Retrieves a specific category by its unique identifier.
        /// </summary>
        /// <param name="id">The category ID to search for.</param>
        /// <returns>The <see cref="Category"/> if found; otherwise, <c>null</c>.</returns>
        Task<Category?> GetCategoryByIdAsync(int id);

        Task<IEnumerable<Category>> GetMainCategoriesAsync(int? excludeCategoryId = null);

        #endregion

        #region Command Methods

        /// <summary>
        /// Creates a new category and logs the admin activity.
        /// </summary>
        /// <param name="category">The category entity to create.</param>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="ipAddress">IP address from which the action was performed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateCategoryAsync(Category category, string userId, string ipAddress);

        /// <summary>
        /// Updates an existing category and logs the admin activity.
        /// </summary>
        /// <param name="category">The category entity with updated values.</param>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="ipAddress">IP address from which the action was performed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the category does not exist.</exception>
        Task UpdateCategoryAsync(Category category, string userId, string ipAddress);

        /// <summary>
        /// Deletes a category by its ID and logs the admin activity.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="ipAddress">IP address from which the action was performed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the category does not exist.</exception>
        Task DeleteCategoryAsync(int id, string userId, string ipAddress);

        #endregion
    }
}
