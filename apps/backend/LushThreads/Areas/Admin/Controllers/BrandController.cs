using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LushThreads.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing brand operations in the admin area.
    /// Requires authentication and admin role.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class BrandController : Controller
    {
        #region Fields

        private readonly IBrandService _brandService;
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BrandController"/> class.
        /// </summary>
        /// <param name="brandService">Service for brand business logic.</param>
        /// <param name="userManager">Identity user manager for retrieving current user.</param>
        public BrandController(IBrandService brandService, UserManager<ApplicationUser> userManager)
        {
            _brandService = brandService;
            _userManager = userManager;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Displays the list of all brands.
        /// </summary>
        /// <returns>View with the list of brands.</returns>
        public async Task<IActionResult> Index()
        {
            var brands = await _brandService.GetAllBrandsAsync();
            return View(brands);
        }

        /// <summary>
        /// Displays the upsert (create/edit) form for a brand.
        /// If id is 0, shows create form; otherwise shows edit form with brand data.
        /// </summary>
        /// <param name="id">The brand ID (0 for new brand).</param>
        /// <returns>The upsert view with the brand model.</returns>
        public async Task<IActionResult> Upsert(int id)
        {
            if (id == 0)
                return View(new Brand()); // Create new brand

            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
                return NotFound();

            return View(brand); // Edit existing brand
        }

        /// <summary>
        /// Handles the creation or update of a brand.
        /// </summary>
        /// <param name="obj">The brand data submitted from the form.</param>
        /// <returns>Redirects to Index on success, or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Brand obj)
        {
            // Step 1: Retrieve the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Step 2: Validate model state
            if (!ModelState.IsValid)
                return View(obj);

            try
            {
                // Step 3: Perform create or update via service
                if (obj.Brand_Id == 0)
                {
                    await _brandService.CreateBrandAsync(obj, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Success"] = "Brand Added successfully";
                }
                else
                {
                    await _brandService.UpdateBrandAsync(obj, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Success"] = $"'{obj.Brand_Name}' Brand updated successfully";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log exception if needed; service already logs, but we can add controller-level logging.
                TempData["Error"] = "An error occurred while saving the brand.";
                return View(obj);
            }
        }

        /// <summary>
        /// Deletes a brand by its ID.
        /// </summary>
        /// <param name="id">The ID of the brand to delete.</param>
        /// <returns>Redirects to Index with a success or error message.</returns>
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _brandService.DeleteBrandAsync(id, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = "Brand deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Oops! Something went wrong. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}