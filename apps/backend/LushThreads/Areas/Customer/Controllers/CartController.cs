using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Cart;
using LushThreads.Domain.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LushThreads.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : BaseController
    {
        #region Fields

        private readonly ICartService _cartService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CartController"/> class.
        /// </summary>
        /// <param name="cartService">Service for cart operations.</param>
        /// <param name="deviceTrackingService">Service for tracking user devices (passed to base controller).</param>
        /// <param name="userManager">Identity user manager (passed to base controller).</param>
        public CartController(
            ICartService cartService,
            IDeviceTrackingService deviceTrackingService,
            UserManager<ApplicationUser> userManager)
            : base(userManager, deviceTrackingService)
        {
            _cartService = cartService;
        }

        #endregion


        #region Cart Views

        /// <summary>
        /// Displays the user's shopping cart.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _cartService.GetCartItemsAsync(user.Id);
            ViewBag.CartCount = await _cartService.GetCartCountAsync(user.Id);

            var (subtotal, shippingFee, tax, total) = CalculateOrderTotals(cartItems);

            var viewModel = new CartViewModel
            {
                Items = cartItems,
                OrderHeader = new OrderHeader
                {
                    OrderDate = DateTime.Now,
                    ShippingDate = DateTime.Now.AddDays(3),
                    Subtotal = subtotal,
                    ShippingFee = shippingFee,
                    Tax = tax,
                    OrderTotal = total,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    StreetAddress = user.StreetAddress,
                    Country = user.Country,
                    PostalCode = user.PostalCode
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays the cart summary with order details.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            var user = await _userManager.GetUserAsync(User);
            var viewModel = await _cartService.GetCartSummaryAsync(user.Id);

            viewModel.OrderHeader.Name = user.Name;
            viewModel.OrderHeader.PhoneNumber = user.PhoneNumber;
            viewModel.OrderHeader.StreetAddress = user.StreetAddress;
            viewModel.OrderHeader.Country = user.Country;
            viewModel.OrderHeader.PostalCode = user.PostalCode;
            viewModel.User = user;

            ViewBag.CartCount = viewModel.Items.Sum(i => i.Quantity);
            return View(viewModel);
        }

        /// <summary>
        /// Processes the cart summary and initiates payment.
        /// </summary>
        [HttpPost]
        [ActionName("Summary")]
        [Route("Customer/Cart/Summary")]
        public async Task<IActionResult> SummaryPOST(CartViewModel viewModel, string selectedPaymentMethod, bool isFinalSubmission = false)
        {
            var user = await _userManager.GetUserAsync(User);
            viewModel.User = user;

            // If not final submission and payment method selected, update user's payment method
            if (!isFinalSubmission && !string.IsNullOrEmpty(selectedPaymentMethod))
            {
                // Update user's payment method (keep this in controller because it's UserManager)
                user.PaymentMehtod = selectedPaymentMethod;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = "Payment method updated successfully!";
                return RedirectToAction(nameof(Summary));
            }

            // Process order
            var (success, message, orderId, redirectUrl) = await _cartService.ProcessOrderAsync(
                user.Id,
                viewModel,
                selectedPaymentMethod,
                HttpContext.Request
            );

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(redirectUrl))
                return Redirect(redirectUrl); // Stripe checkout

            // Cash on delivery: redirect to confirmation
            return RedirectToAction("OrderConfirmation", new { id = orderId });
        }

        /// <summary>
        /// Confirms the order after payment.
        /// </summary>
        public async Task<IActionResult> OrderConfirmation(int id, string session_id)
        {
            var user = await _userManager.GetUserAsync(User);

            var (success, message, orderHeader, orderDetails) = await _cartService.ConfirmOrderAsync(id, session_id, user.Id);

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CartCount = 0; // Cart is cleared
            return View(new OrderVM
            {
                OrderHeader = orderHeader,
                OrderDetails = orderDetails
            });
        }

        #endregion

        #region Cart Actions (AJAX or POST)

        /// <summary>
        /// Adds an item to the cart via AJAX.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string size = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var (success, message, cartCount) = await _cartService.AddToCartAsync(user.Id, productId, quantity, size);
            return Json(new { success, message, cartCount });
        }

        /// <summary>
        /// Removes an item from the cart.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            await _cartService.RemoveFromCartAsync(id, user.Id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Increases quantity of a cart item.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> IncreaseQuantity(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            await _cartService.IncreaseQuantityAsync(id, user.Id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Decreases quantity of a cart item.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            await _cartService.DecreaseQuantityAsync(id, user.Id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Clears all items from the cart.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var user = await _userManager.GetUserAsync(User);
            await _cartService.ClearCartAsync(user.Id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates the size of a cart item via AJAX.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateSize(int itemId, string size)
        {
            var user = await _userManager.GetUserAsync(User);
            var (success, message) = await _cartService.UpdateSizeAsync(itemId, user.Id, size);
            return Json(new { success, message });
        }

        /// <summary>
        /// Updates the user's payment method (from cart).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangePayment(string PaymentMethod)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.PaymentMehtod = PaymentMethod;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = "Payment method updated successfully!";
            }
            else
            {
                TempData["Error"] = "User not found";
            }
            return RedirectToAction(nameof(Summary));
        }

        #endregion

        #region Private Helper

        private (double Subtotal, double ShippingFee, double Tax, double Total) CalculateOrderTotals(IEnumerable<CartItem> cartItems)
        {
            double subtotal = (double)cartItems.Sum(i => i.Product.Product_Price * i.Quantity);
            double shippingFee = subtotal > 100 ? 0 : 10;
            double tax = subtotal * 0.08;
            double total = subtotal + shippingFee + tax;
            return (subtotal, shippingFee, tax, total);
        }

        #endregion
    }
}