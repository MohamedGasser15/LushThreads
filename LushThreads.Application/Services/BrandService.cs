using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service class responsible for business logic related to Brand entities.
    /// Implements <see cref="IBrandService"/>.
    /// </summary>
    public class BrandService : IBrandService
    {
        #region Fields

        private readonly IBrandRepository _brandRepository;
        private readonly IAdminActivityService _adminActivityService;
        private readonly ILogger<BrandService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BrandService"/> class.
        /// </summary>
        /// <param name="brandRepository">Repository for Brand operations.</param>
        /// <param name="adminActivityRepository">Repository for AdminActivity logging.</param>
        /// <param name="logger">Logger for service-level logging.</param>
        public BrandService(
            IBrandRepository brandRepository,
            IAdminActivityService adminActivityService,
            ILogger<BrandService> logger)
        {
            _brandRepository = brandRepository;
            _adminActivityService = adminActivityService;
            _logger = logger;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            _logger.LogDebug("Retrieving all brands from repository.");
            var brands = await _brandRepository.GetAllAsync();
            _logger.LogDebug("Retrieved {Count} brands.", brands?.Count() ?? 0);
            return brands;
        }

        /// <inheritdoc />
        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            _logger.LogDebug("Retrieving brand with ID {BrandId}.", id);
            var brand = await _brandRepository.GetAsync(b => b.Brand_Id == id);
            if (brand == null)
            {
                _logger.LogWarning("Brand with ID {BrandId} not found.", id);
            }
            return brand;
        }

        /// <inheritdoc />
        public async Task CreateBrandAsync(Brand brand, string userId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Starting creation of brand '{BrandName}' by user {UserId}.", brand.Brand_Name, userId);

                // Step 1: Add the brand entity to the database
                await _brandRepository.CreateAsync(brand);
                _logger.LogDebug("Brand entity added to repository. Generated ID: {BrandId}.", brand.Brand_Id);

                // Step 2: Log the admin activity
                await _adminActivityService.LogActivityAsync(
                    userId,
                    "AddBrand",
                    $"Add Brand (Id: {brand.Brand_Id})",
                    ipAddress
                );
                _logger.LogDebug("Admin activity logged for brand creation.");

                _logger.LogInformation("Brand created successfully with ID {BrandId} by user {UserId}.", brand.Brand_Id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating brand '{BrandName}' by user {UserId}.", brand.Brand_Name, userId);
                throw; // Re-throw to let the controller handle it
            }
        }

        /// <inheritdoc />
        public async Task UpdateBrandAsync(Brand brand, string userId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Starting update of brand ID {BrandId} by user {UserId}.", brand.Brand_Id, userId);

                // Step 1: Verify the brand exists
                var existing = await _brandRepository.GetAsync(b => b.Brand_Id == brand.Brand_Id);
                if (existing == null)
                {
                    _logger.LogWarning("Brand with ID {BrandId} not found for update.", brand.Brand_Id);
                    throw new InvalidOperationException($"Brand with id {brand.Brand_Id} not found.");
                }

                // Step 2: Update the brand entity
                await _brandRepository.UpdateAsync(brand);
                _logger.LogDebug("Brand entity updated in repository.");

                // Step 3: Log the admin activity
                await _adminActivityService.LogActivityAsync(
                    userId,
                    "UpdateBrand",
                    $"Update Brand (Id: {brand.Brand_Id})",
                    ipAddress
                );
                _logger.LogDebug("Admin activity logged for brand update.");

                _logger.LogInformation("Brand ID {BrandId} updated successfully by user {UserId}.", brand.Brand_Id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating brand ID {BrandId} by user {UserId}.", brand.Brand_Id, userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteBrandAsync(int id, string userId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Starting deletion of brand ID {BrandId} by user {UserId}.", id, userId);

                // Step 1: Retrieve the brand to delete
                var brand = await _brandRepository.GetAsync(b => b.Brand_Id == id);
                if (brand == null)
                {
                    _logger.LogWarning("Brand with ID {BrandId} not found for deletion.", id);
                    throw new InvalidOperationException($"Brand with id {id} not found.");
                }

                // Step 2: Delete the brand entity
                await _brandRepository.DeleteAsync(brand);
                _logger.LogDebug("Brand entity deleted from repository.");

                // Step 3: Log the admin activity
                await _adminActivityService.LogActivityAsync(
                    userId,
                    "RemoveBrand",
                    $"Remove Brand (Id: {id})",
                    ipAddress
                );
                _logger.LogDebug("Admin activity logged for brand deletion.");

                _logger.LogInformation("Brand ID {BrandId} deleted successfully by user {UserId}.", id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting brand ID {BrandId} by user {UserId}.", id, userId);
                throw;
            }
        }

        #endregion
    }
}