using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Home;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LushThreads.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        #region Fields

        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;
        private readonly IRepository<CartItem> _cartItemRepository;

        #endregion

        #region Constructor

        public HomeController(
            ILogger<HomeController> logger,
            IHomeService homeService,
            IRepository<CartItem> cartItemRepository)
        {
            _logger = logger;
            _homeService = homeService;
            _cartItemRepository = cartItemRepository;
        }

        #endregion

        #region Home Page

        /// <summary>
        /// Displays home page with featured and new arrival products.
        /// </summary>
        public async Task<IActionResult> Home(int? categoryId = null)
        {
            ViewBag.CartCount = GetCartCount();

            var viewModel = await _homeService.GetHomeViewModelAsync(categoryId);

            // Prepare categories and brands (static data)
            var specificCategories = new[] { "Bags", "Jackets", "Hats", "Shoes", "Pantalons", "Dresses", "Shorts", "Hoodie" };
            var categories = specificCategories
                .Select(c => new
                {
                    Name = c,
                    ImgUrl = $"/img/categories/{c.ToLower()}.jpg",
                    Link = Url.Action("ShopByChildCategory", "Home", new { area = "Customer", category = c.ToLower() })
                })
                .ToList();

            var specificBrands = new[] { "Nike", "Adidas", "Zara", "H&M", "Puma", "Levi's", "Lacoste", "Local Brands" };
            var brands = specificBrands
                .Select(b => new
                {
                    Name = b,
                    ImgUrl = $"/img/brands/{b.ToLower().Replace("'", "").Replace("&", "").Replace(" ", "")}.jpg",
                    Link = Url.Action("ShopByBrand", "Home", new { area = "Customer", brand = b.ToLower().Replace("'", "").Replace("&", "").Replace(" ", "") })
                })
                .ToList();

            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.Categories = categories;
            ViewBag.Brands = brands;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var jsonResult = await _homeService.GetHomeProductsJsonAsync(categoryId);
                return Json(jsonResult);
            }

            return View(viewModel);
        }

        #endregion

        #region Shop

        /// <summary>
        /// Displays paginated shop page with optional category filter.
        /// </summary>
        public async Task<IActionResult> Shop(int? categoryId = null, int page = 1, int pageSize = 8)
        {
            ViewBag.CartCount = GetCartCount();

            page = Math.Max(1, page);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

            var (products, totalPages, totalProducts) = await _homeService.GetShopProductsAsync(categoryId, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.CurrentCategoryId = categoryId;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { products, currentPage = page, totalPages, pageSize, currentCategoryId = categoryId });
            }

            return View(products);
        }

        #endregion

        #region Product Details

        /// <summary>
        /// Displays detailed view of a single product.
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            ViewBag.CartCount = GetCartCount();

            var product = await _homeService.GetProductDetailsAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        /// <summary>
        /// Fetches product details by ID for JSON response.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            ViewBag.CartCount = GetCartCount();

            var product = await _homeService.GetProductDetailsJsonAsync(productId);
            if (product == null)
                return Json(new { success = false, message = "Product not found" });

            return Json(new { success = true, product });
        }

        /// <summary>
        /// Fetches available sizes for a product.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductSizes(int productId)
        {
            ViewBag.CartCount = GetCartCount();

            var sizes = await _homeService.GetProductSizesAsync(productId);
            return Json(new { success = true, sizes });
        }

        #endregion

        #region Category & Brand Filtering

        /// <summary>
        /// Displays products by main category.
        /// </summary>
        public async Task<IActionResult> ShopByCategory(string category)
        {
            ViewBag.CartCount = GetCartCount();

            var (categoryName, products) = await _homeService.GetProductsByCategoryAsync(category);
            if (categoryName == null)
                return NotFound();

            ViewBag.Category = categoryName;
            return View(products ?? new System.Collections.Generic.List<Product>());
        }

        /// <summary>
        /// Displays products by child category.
        /// </summary>
        public async Task<IActionResult> ShopByChildCategory(string category)
        {
            ViewBag.CartCount = GetCartCount();

            var (categoryName, products) = await _homeService.GetProductsByChildCategoryAsync(category);
            if (categoryName == null)
                return NotFound();

            ViewBag.Category = categoryName;
            return View("ShopByCategory", products ?? new System.Collections.Generic.List<Product>());
        }

        /// <summary>
        /// Displays products by brand.
        /// </summary>
        public async Task<IActionResult> ShopByBrand(string brand)
        {
            ViewBag.CartCount = GetCartCount();

            var (brandName, products) = await _homeService.GetProductsByBrandAsync(brand);
            if (brandName == null)
                return NotFound();

            ViewBag.Brand = brandName;
            return View("ShopByBrand", products ?? new System.Collections.Generic.List<Product>());
        }

        #endregion

        #region Search

        /// <summary>
        /// Handles product search by name or description.
        /// </summary>
        public async Task<IActionResult> Search(string searchTerm)
        {
            ViewBag.CartCount = GetCartCount();

            var viewModel = new SearchViewModel { SearchTerm = searchTerm };
            viewModel.Results = await _homeService.SearchProductsAsync(searchTerm);

            return View(viewModel);
        }

        #endregion

        #region Static Pages

        /// <summary>
        /// Displays the About page.
        /// </summary>
        public IActionResult About()
        {
            ViewBag.CartCount = GetCartCount();
            return View();
        }

        /// <summary>
        /// Displays the Contact page.
        /// </summary>
        public IActionResult Contact()
        {
            ViewBag.CartCount = GetCartCount();
            return View();
        }

        #endregion

        #region Error

        /// <summary>
        /// Handles error display.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ShowRequestId = !string.IsNullOrEmpty(Activity.Current?.Id ?? HttpContext.TraceIdentifier)
            };

            if (statusCode.HasValue)
            {
                ViewData["StatusCode"] = statusCode.Value;
            }

            return View(errorViewModel);
        }

        #endregion

        #region Private Helper

        /// <summary>
        /// Retrieves cart item count for authenticated users.
        /// </summary>
        private int GetCartCount()
        {
            if (!User.Identity.IsAuthenticated)
                return 0;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return 0;

            // Use repository directly (or inject service if needed)
            return _cartItemRepository.GetAllAsync(filter: c => c.UserId == userId).Result.Count;
        }

        #endregion
    }
}