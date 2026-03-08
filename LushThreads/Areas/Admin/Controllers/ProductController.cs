using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace LushThreads.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing product operations in the admin area.
    /// Requires authentication and admin role.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class ProductController : Controller
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="productService">Service for product business logic.</param>
        /// <param name="userManager">Identity user manager for retrieving current user.</param>
        public ProductController(IProductService productService, UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _userManager = userManager;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Displays the list of all products.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        /// <summary>
        /// Displays the form for creating or editing a product.
        /// </summary>
        /// <param name="id">Product ID (0 for new).</param>
        public async Task<IActionResult> Upsert(int id)
        {
            var viewModel = await _productService.GetProductViewModelForUpsertAsync(id);

            if (id != 0 && viewModel.Product == null)
                return NotFound();

            return View(viewModel);
        }

        /// <summary>
        /// Handles the creation or update of a product with image and stock handling.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductViewModel viewModel, IFormFile? file, string? croppedImageData)
        {
            // Step 1: Retrieve current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

           

            try
            {
                // Step 3: Perform upsert via service
                int productId = await _productService.UpsertProductAsync(
                    viewModel,
                    file,
                    croppedImageData,
                    user.Id,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                TempData["Success"] = viewModel.Product.Product_Id == 0
                    ? "Product Added successfully"
                    : $"'{viewModel.Product.Product_Name}' updated successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                // Repopulate dropdowns
                var freshViewModel = await _productService.GetProductViewModelForUpsertAsync(viewModel.Product?.Product_Id ?? 0);
                viewModel.BrandList = freshViewModel.BrandList;
                viewModel.CategoryList = freshViewModel.CategoryList;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log exception (service already logs)
                TempData["Error"] = "An error occurred while saving the product.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Deletes a product and its associated stocks and image.
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _productService.DeleteProductAsync(id, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Oops! Something went wrong. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Toggles the featured status of a product.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeFeatured(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                bool isFeatured = await _productService.ToggleFeaturedAsync(id, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = isFeatured
                    ? "Product has been added to featured items!"
                    : "Product has been removed from featured items!";
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