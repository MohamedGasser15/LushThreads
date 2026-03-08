using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Defines the contract for product-related business operations.
    /// </summary>
    public interface IProductService
    {
        #region Query Methods

        /// <summary>
        /// Retrieves all products with related data (Category, Brand, Stocks) ordered by ID descending.
        /// </summary>
        /// <returns>Collection of products.</returns>
        Task<IEnumerable<Product>> GetAllProductsAsync();

        /// <summary>
        /// Retrieves a product by its ID including stocks, category, and brand.
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <returns>The product if found; otherwise null.</returns>
        Task<Product?> GetProductByIdAsync(int id);

        /// <summary>
        /// Prepares the ProductViewModel for upsert view (populates BrandList, CategoryList).
        /// </summary>
        /// <param name="id">Product ID (0 for new).</param>
        /// <returns>Populated ProductViewModel.</returns>
        Task<ProductViewModel> GetProductViewModelForUpsertAsync(int id);

        #endregion

        #region Command Methods

        /// <summary>
        /// Creates or updates a product with image and stock handling.
        /// </summary>
        /// <param name="viewModel">ProductViewModel containing product data, stocks, and image info.</param>
        /// <param name="file">Uploaded image file.</param>
        /// <param name="croppedImageData">Cropped image base64 data.</param>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="ipAddress">IP address of the user.</param>
        /// <returns>The created/updated product ID.</returns>
        Task<int> UpsertProductAsync(ProductViewModel viewModel, IFormFile? file, string? croppedImageData, string userId, string? ipAddress);

        /// <summary>
        /// Deletes a product and its associated stocks and image.
        /// </summary>
        /// <param name="id">Product ID to delete.</param>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="ipAddress">IP address of the user.</param>
        Task DeleteProductAsync(int id, string userId, string? ipAddress);

        /// <summary>
        /// Toggles the featured status of a product.
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="ipAddress">IP address of the user.</param>
        /// <returns>The new featured status.</returns>
        Task<bool> ToggleFeaturedAsync(int id, string userId, string? ipAddress);

        #endregion
    }
}
