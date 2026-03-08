using LushThreads.Domain.Entites;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Defines the contract for brand-related business operations.
    /// </summary>
    public interface IBrandService
    {
        #region Query Methods

        /// <summary>
        /// Retrieves all brands from the system.
        /// </summary>
        /// <returns>A collection of all <see cref="Brand"/> entities.</returns>
        Task<IEnumerable<Brand>> GetAllBrandsAsync();

        /// <summary>
        /// Retrieves a specific brand by its unique identifier.
        /// </summary>
        /// <param name="id">The brand ID to search for.</param>
        /// <returns>The <see cref="Brand"/> if found; otherwise, <c>null</c>.</returns>
        Task<Brand?> GetBrandByIdAsync(int id);

        #endregion

        #region Command Methods

        /// <summary>
        /// Creates a new brand and logs the admin activity.
        /// </summary>
        /// <param name="brand">The brand entity to create.</param>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="ipAddress">IP address from which the action was performed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateBrandAsync(Brand brand, string userId, string ipAddress);

        /// <summary>
        /// Updates an existing brand and logs the admin activity.
        /// </summary>
        /// <param name="brand">The brand entity with updated values.</param>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="ipAddress">IP address from which the action was performed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the brand does not exist.</exception>
        Task UpdateBrandAsync(Brand brand, string userId, string ipAddress);

        /// <summary>
        /// Deletes a brand by its ID and logs the admin activity.
        /// </summary>
        /// <param name="id">The ID of the brand to delete.</param>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="ipAddress">IP address from which the action was performed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the brand does not exist.</exception>
        Task DeleteBrandAsync(int id, string userId, string ipAddress);

        #endregion
    }
}