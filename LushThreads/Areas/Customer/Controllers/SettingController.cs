using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LushThreads.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class SettingController : BaseController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IAdminActivityService _adminActivityService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingController"/> class.
        /// </summary>
        /// <param name="settingService">Service for user settings.</param>
        /// <param name="adminActivityService">Service for logging activities.</param>
        /// <param name="userManager">User manager (passed to base).</param>
        /// <param name="db">Database context (passed to base).</param>
        public SettingController(
            ISettingService settingService,
            IAdminActivityService adminActivityService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db) : base(db, userManager)
        {
            _settingService = settingService;
            _adminActivityService = adminActivityService;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Displays user settings page.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _settingService.GetUserByIdAsync(userId);
            return View(user);
        }

        /// <summary>
        /// Changes user's preferred language and updates session.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeLanguage(string PreferredLanguage)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _settingService.ChangeLanguageAsync(userId, PreferredLanguage);

            if (result.Succeeded)
            {
                HttpContext.Session?.SetString("lang", PreferredLanguage);
                await _adminActivityService.LogActivityAsync(userId, "ChangeLanguage", $"Changed language to {PreferredLanguage}", HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["SuccessMessage"] = "Language changed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to change language";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Changes user's preferred currency.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeCurrency(string Currency)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _settingService.ChangeCurrencyAsync(userId, Currency);

            if (result.Succeeded)
            {
                await _adminActivityService.LogActivityAsync(userId, "ChangeCurrency", $"Changed currency to {Currency}", HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["SuccessMessage"] = "Currency changed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to change currency";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates user's preferred payment method.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangePayment(string PaymentMethod)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _settingService.ChangePaymentMethodAsync(userId, PaymentMethod);

            if (result.Succeeded)
            {
                await _adminActivityService.LogActivityAsync(userId, "ChangePaymentMethod", $"Changed payment method to {PaymentMethod}", HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["SuccessMessage"] = "Payment method updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update payment method";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates user's preferred shipping carriers.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangePreferredCarriers(string PreferredCarriers)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _settingService.ChangePreferredCarriersAsync(userId, PreferredCarriers);

            if (result.Succeeded)
            {
                await _adminActivityService.LogActivityAsync(userId, "ChangePreferredCarriers", $"Changed carriers to {PreferredCarriers}", HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["SuccessMessage"] = "Preferred carriers updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update preferred carriers";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates user's primary address.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrimaryAddress(string userId, string addressValue)
        {
            var result = await _settingService.UpdatePrimaryAddressAsync(userId, addressValue);

            if (result.Succeeded)
            {
                await _adminActivityService.LogActivityAsync(userId, "UpdatePrimaryAddress", $"Updated primary address", HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["SuccessMessage"] = "Primary address updated successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update primary address";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates user's secondary address.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSecondaryAddress(string userId, string addressValue)
        {
            var result = await _settingService.UpdateSecondaryAddressAsync(userId, addressValue);

            if (result.Succeeded)
            {
                await _adminActivityService.LogActivityAsync(userId, "UpdateSecondaryAddress", $"Updated secondary address", HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["SuccessMessage"] = "Secondary address updated successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update secondary address";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Swaps primary and secondary addresses.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryAddress(string userId)
        {
            var result = await _settingService.SetPrimaryAddressAsync(userId);

            if (result.Succeeded)
            {
                await _adminActivityService.LogActivityAsync(userId, "SwapAddresses", $"Swapped primary and secondary addresses", HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["SuccessMessage"] = "Primary address changed successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to change primary address";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Deletes user's secondary address.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSecondaryAddress(string userId)
        {
            var result = await _settingService.DeleteSecondaryAddressAsync(userId);

            if (result.Succeeded)
            {
                await _adminActivityService.LogActivityAsync(userId, "DeleteSecondaryAddress", $"Deleted secondary address", HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["SuccessMessage"] = "Secondary address removed";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove address";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays shipping settings page.
        /// </summary>
        public IActionResult Shipping()
        {
            return View();
        }

        /// <summary>
        /// Displays notifications settings page.
        /// </summary>
        public IActionResult Notifications()
        {
            return View();
        }

        /// <summary>
        /// Displays privacy settings page.
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        #endregion
    }
}