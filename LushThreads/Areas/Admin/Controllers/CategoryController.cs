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
    /// Controller for managing category operations in the admin area.
    /// Requires authentication and admin role.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class CategoryController : Controller
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryController"/> class.
        /// </summary>
        /// <param name="categoryService">Service for category business logic.</param>
        /// <param name="userManager">Identity user manager for retrieving current user.</param>
        public CategoryController(ICategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            _categoryService = categoryService;
            _userManager = userManager;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Displays the list of all categories.
        /// </summary>
        /// <returns>View with the list of categories.</returns>
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        /// <summary>
        /// Displays the upsert (create/edit) form for a category.
        /// If id is 0, shows create form; otherwise shows edit form with category data.
        /// </summary>
        /// <param name="id">The category ID (0 for new category).</param>
        /// <returns>The upsert view with the category model.</returns>
        public async Task<IActionResult> Upsert(int id)
        {
            if (id == 0)
                return View(new Category()); // Create new category

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category); // Edit existing category
        }

        /// <summary>
        /// Handles the creation or update of a category.
        /// </summary>
        /// <param name="obj">The category data submitted from the form.</param>
        /// <returns>Redirects to Index on success, or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Category obj)
        {
            // Step 1: Retrieve the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();



            try
            {
                // Step 3: Perform create or update via service
                if (obj.Category_Id == 0)
                {
                    await _categoryService.CreateCategoryAsync(obj, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Success"] = "Category Added successfully";
                }
                else
                {
                    await _categoryService.UpdateCategoryAsync(obj, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Success"] = $"'{obj.Category_Name}' Category updated successfully";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log exception if needed; service already logs.
                TempData["Error"] = "An error occurred while saving the category.";
                return View(obj);
            }
        }

        /// <summary>
        /// Deletes a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>Redirects to Index with a success or error message.</returns>
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _categoryService.DeleteCategoryAsync(id, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = "Category deleted successfully!";
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