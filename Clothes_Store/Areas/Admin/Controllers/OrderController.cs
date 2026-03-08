using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LushThreads.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class OrderController : Controller
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion

        #region Constructor

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Displays the list of all orders.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        /// <summary>
        /// Displays order details.
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var orderVM = await _orderService.GetOrderDetailsAsync(id);
            if (orderVM == null || orderVM.OrderHeader == null)
                return NotFound();

            return View(orderVM);
        }

        /// <summary>
        /// Updates customer information.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateCustomerInfo(int orderId, string name, string phoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _orderService.UpdateCustomerInfoAsync(orderId, name, phoneNumber, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = "Customer information updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        /// <summary>
        /// Updates shipping information.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateShippingInfo(int orderId, string streetAddress, string country, string postalCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _orderService.UpdateShippingInfoAsync(orderId, streetAddress, country, postalCode, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = "Shipping information updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        /// <summary>
        /// Updates tracking information.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTracking(int id, string trackingNumber, string carrier)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _orderService.UpdateTrackingInfoAsync(id, trackingNumber, carrier, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = "Tracking information updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Approves an order.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _orderService.ApproveOrderAsync(id, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = $"Order #{id} approved successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Order approval failed: {ex.Message}";
            }

            return Redirect(returnUrl ?? Url.Action(nameof(Index)));
        }

        /// <summary>
        /// Cancels an order.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _orderService.CancelOrderAsync(id, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = $"Order #{id} cancelled successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Order cancellation failed: {ex.Message}";
            }

            return Redirect(returnUrl ?? Url.Action(nameof(Details), new { id }));
        }

        /// <summary>
        /// Marks an order as shipped.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShipOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _orderService.ShipOrderAsync(id, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = $"Order #{id} shipped successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Marks an order as delivered.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsDelivered(int id, string returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                await _orderService.DeliverOrderAsync(id, user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Success"] = $"Order #{id} delivered successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return Redirect(returnUrl ?? Url.Action(nameof(Index)));
        }

        /// <summary>
        /// Removes cancelled or refunded orders.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCanceledRefundedOrders(int id, string returnUrl = null)
        {
            try
            {
                await _orderService.RemoveCanceledOrRefundedOrderAsync(id, returnUrl);
                TempData["Success"] = $"Successfully removed order #{id}!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("/Order/Details/"))
                return RedirectToAction(nameof(Index));

            return Redirect(returnUrl ?? Url.Action(nameof(Index)));
        }

        #endregion
    }
}