using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service class responsible for business logic related to Category entities.
    /// Implements <see cref="ICategoryService"/>.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        #region Fields

        private readonly ICategoryRepository _categoryRepository;
        private readonly IAdminActivityService _adminActivityService;
        private readonly ILogger<CategoryService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        /// <param name="categoryRepository">Repository for Category operations.</param>
        /// <param name="adminActivityRepository">Repository for AdminActivity logging.</param>
        /// <param name="logger">Logger for service-level logging.</param>
        public CategoryService(
            ICategoryRepository categoryRepository,
            IAdminActivityService adminActivityService,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _adminActivityService = adminActivityService;
            _logger = logger;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            _logger.LogDebug("Retrieving all categories from repository.");
            var categories = await _categoryRepository.GetAllAsync();
            _logger.LogDebug("Retrieved {Count} categories.", categories?.Count() ?? 0);
            return categories;
        }

        /// <inheritdoc />
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            _logger.LogDebug("Retrieving category with ID {CategoryId}.", id);
            var category = await _categoryRepository.GetAsync(c => c.Category_Id == id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
            }
            return category;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Category>> GetMainCategoriesAsync(int? excludeCategoryId = null)
        {
            _logger.LogDebug("Retrieving main categories from repository.");
            var categories = await _categoryRepository.GetAllAsync(
                filter: c => c.ParentCategoryId == null && (!excludeCategoryId.HasValue || c.Category_Id != excludeCategoryId.Value)
            );
            return categories;
        }

        /// <inheritdoc />
        public async Task CreateCategoryAsync(Category category, string userId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Starting creation of category '{CategoryName}' by user {UserId}.", category.Category_Name, userId);

                // Step 1: Add the category entity to the database
                await _categoryRepository.CreateAsync(category);
                _logger.LogDebug("Category entity added to repository. Generated ID: {CategoryId}.", category.Category_Id);

                // Step 2: Log the admin activity
                await _adminActivityService.LogActivityAsync(
                    userId,
                    "AddCategory",
                    $"Add Category (Id: {category.Category_Id})",
                    ipAddress
                );
                _logger.LogDebug("Admin activity logged for category creation.");

                _logger.LogInformation("Category created successfully with ID {CategoryId} by user {UserId}.", category.Category_Id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category '{CategoryName}' by user {UserId}.", category.Category_Name, userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task UpdateCategoryAsync(Category category, string userId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Starting update of category ID {CategoryId} by user {UserId}.", category.Category_Id, userId);

                // Step 1: Verify the category exists
                var existing = await _categoryRepository.GetAsync(c => c.Category_Id == category.Category_Id);
                if (existing == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for update.", category.Category_Id);
                    throw new InvalidOperationException($"Category with id {category.Category_Id} not found.");
                }

                // Step 2: Update the category entity
                await _categoryRepository.UpdateAsync(category);
                _logger.LogDebug("Category entity updated in repository.");

                // Step 3: Log the admin activity
                await _adminActivityService.LogActivityAsync(
                    userId,
                    "UpdateCategory",
                    $"Update Category (Id: {category.Category_Id})",
                    ipAddress
                );
                _logger.LogDebug("Admin activity logged for category update.");

                _logger.LogInformation("Category ID {CategoryId} updated successfully by user {UserId}.", category.Category_Id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category ID {CategoryId} by user {UserId}.", category.Category_Id, userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteCategoryAsync(int id, string userId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Starting deletion of category ID {CategoryId} by user {UserId}.", id, userId);

                // Step 1: Retrieve the category to delete
                var category = await _categoryRepository.GetAsync(c => c.Category_Id == id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for deletion.", id);
                    throw new InvalidOperationException($"Category with id {id} not found.");
                }

                // Step 2: Delete the category entity
                await _categoryRepository.DeleteAsync(category);
                _logger.LogDebug("Category entity deleted from repository.");

                // Step 3: Log the admin activity
                await _adminActivityService.LogActivityAsync(
                    userId,
                    "DeleteCategory",
                    $"Delete Category (Id: {id})",
                    ipAddress
                );
                _logger.LogDebug("Admin activity logged for category deletion.");

                _logger.LogInformation("Category ID {CategoryId} deleted successfully by user {UserId}.", id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category ID {CategoryId} by user {UserId}.", id, userId);
                throw;
            }
        }

        #endregion
    }
}