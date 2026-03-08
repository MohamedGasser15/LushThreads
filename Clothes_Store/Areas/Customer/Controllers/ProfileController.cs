using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Auth;
using LushThreads.Domain.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LushThreads.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ProfileController : BaseController
    {
        #region Fields

        private readonly IProfileService _profileService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<ProfileController> _logger;

        #endregion

        #region Constructor

        public ProfileController(
            IProfileService profileService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            ILogger<ProfileController> logger)
            : base(null, userManager) // BaseController expects db and userManager; we pass null for db (not used)
        {
            _profileService = profileService;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        #endregion

        #region Profile

        /// <summary>
        /// Displays the user's profile information.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null)
                return RedirectToAction("Edit");

            return View(profile);
        }

        /// <summary>
        /// Updates the user's name.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateName(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _profileService.UpdateNameAsync(userId, model.Name);
                TempData["SuccessMessage"] = "Name updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates the user's email.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateEmail(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _profileService.UpdateEmailAsync(userId, model.Email);
                TempData["SuccessMessage"] = "Email updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates the user's phone number.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdatePhone(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _profileService.UpdatePhoneAsync(userId, model.PhoneNumber);
                TempData["SuccessMessage"] = "Phone number updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates the user's address.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateAddress(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _profileService.UpdateAddressAsync(userId, model.Address);
                TempData["SuccessMessage"] = "Address updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates the user's postal code.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdatePostalCode(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _profileService.UpdatePostalCodeAsync(userId, model.PostalCode);
                TempData["SuccessMessage"] = "Postal code updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates the user's country.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateCountry(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _profileService.UpdateCountryAsync(userId, model.Country);
                TempData["SuccessMessage"] = "Country updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Security

        /// <summary>
        /// Displays security settings and recent activities.
        /// </summary>
        public async Task<IActionResult> Security()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await _profileService.GetSecurityViewModelAsync(userId);
            if (model == null)
                return NotFound();

            return View(model);
        }

        /// <summary>
        /// Sends email verification code.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailVerificationCode()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var code = await _profileService.GenerateEmailVerificationCodeAsync(userId);
                TempData["EmailVerificationCode"] = code;
                return RedirectToAction("VerifyEmailCode", new { userId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Security");
            }
        }

        /// <summary>
        /// Displays page to input email verification code.
        /// </summary>
        [HttpGet]
        public IActionResult VerifyEmailCode(string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null)
                return NotFound();

            return View(new VerifyEmailCodeViewModel { UserId = user.Id, Email = user.Email });
        }

        /// <summary>
        /// Verifies email confirmation code.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmailCode(VerifyEmailCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null)
                    model.Email = user.Email;
                ModelState.AddModelError("Code", "Please enter a valid 6-digit code.");
                return View(model);
            }

            var storedCode = TempData.Peek("EmailVerificationCode")?.ToString();
            var isValid = await _profileService.VerifyEmailCodeAsync(model.UserId, model.Code, storedCode);

            if (!isValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null)
                    model.Email = user.Email;
                ModelState.AddModelError("Code", "Invalid or expired verification code.");
                return View(model);
            }

            TempData.Remove("EmailVerificationCode");

            // Mark email as confirmed
            var userToConfirm = await _userManager.FindByIdAsync(model.UserId);
            userToConfirm.EmailConfirmed = true;
            await _userManager.UpdateAsync(userToConfirm);

            TempData["SuccessMessage"] = "Email verified successfully!";
            return RedirectToAction("Security");
        }

        /// <summary>
        /// Displays the change password page.
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        /// <summary>
        /// Processes password change.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                await _profileService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword, ipAddress);

                // Generate password reset link for email
                var user = await _userManager.FindByIdAsync(userId);
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetLink = Url.Action("ResetPassword", "Account",
                    new { userId = user.Id, code = resetToken }, HttpContext.Request.Scheme);

                // Send confirmation email with template (original logic)
                var emailBody = _emailTemplateService.GeneratePasswordChangeEmail(user, ipAddress, System.Net.Dns.GetHostName(), DateTime.Now, passwordResetLink);
                await _emailSender.SendEmailAsync(user.Email, "Your password has been changed", emailBody);

                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Security");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        /// <summary>
        /// Initiates two-factor authentication setup.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Enable2FA()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var code = await _profileService.Initiate2FASetupAsync(userId);
                HttpContext.Session.SetString("2FA_Code", code);
                HttpContext.Session.SetString("2FA_User", userId);
                HttpContext.Session.SetString("2FA_Expiry", DateTime.Now.AddMinutes(5).ToString());

                var user = await _userManager.FindByIdAsync(userId);
                return View(new TwoFactorSetupViewModel { Email = user.Email });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Security");
            }
        }

        /// <summary>
        /// Verifies 2FA code and enables two-factor authentication.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Verify2FACode(string code)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var storedCode = HttpContext.Session.GetString("2FA_Code");
            var expiryStr = HttpContext.Session.GetString("2FA_Expiry");

            if (!DateTime.TryParse(expiryStr, out var expiry))
                expiry = DateTime.MinValue;

            var isValid = await _profileService.VerifyAndEnable2FAAsync(userId, code, storedCode, expiry);

            if (isValid)
            {
                var user = await _userManager.FindByIdAsync(userId);
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                HttpContext.Session.Remove("2FA_Code");
                HttpContext.Session.Remove("2FA_Expiry");
                TempData["SuccessMessage"] = "Two-factor authentication enabled successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid or expired verification code.";
            }

            return RedirectToAction("Security");
        }

        /// <summary>
        /// Disables two-factor authentication.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2FA()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _profileService.Disable2FAAsync(userId);
                TempData["SuccessMessage"] = "Two-factor authentication disabled successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Security");
        }

        /// <summary>
        /// Resends 2FA verification code (simply redirects to Enable2FA).
        /// </summary>
        [HttpPost]
        public IActionResult Resend2FACode()
        {
            return RedirectToAction("Enable2FA");
        }

        #endregion

        #region Devices

        /// <summary>
        /// Displays connected devices for the user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ManageDevices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var devices = await _profileService.GetUserDevicesAsync(userId);
            return View(devices);
        }

        /// <summary>
        /// Removes a specific device.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDevice(string deviceId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentDeviceToken = Request.Cookies["DeviceToken"];

            try
            {
                await _profileService.RemoveDeviceAsync(userId, Guid.Parse(deviceId), currentDeviceToken);
                TempData["Success"] = "Device removed successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("ManageDevices");
        }

        /// <summary>
        /// Removes inactive devices older than 30 days.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveInactiveDevices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentDeviceToken = Request.Cookies["DeviceToken"];

            try
            {
                var count = await _profileService.RemoveInactiveDevicesAsync(userId, currentDeviceToken);
                if (count > 0)
                    TempData["Success"] = $"Removed {count} inactive devices!";
                else
                    TempData["Info"] = "No inactive devices to remove";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("ManageDevices");
        }

        #endregion

        #region Orders

        /// <summary>
        /// Displays paginated user orders.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Orders(int page = 1, int pageSize = 5)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToPage("/Account/Login");

            var (orders, totalCount) = await _profileService.GetUserOrdersAsync(userId, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;

            return View(orders);
        }

        /// <summary>
        /// Cancels an order by the user.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                await _profileService.CancelOrderAsync(userId, id, ipAddress);

                // Send cancellation email (optional, service doesn't send email)
                var user = await _userManager.FindByIdAsync(userId);
                var orderLink = Url.Action("Orders", "Profile", new { area = "Customer" }, HttpContext.Request.Scheme);
                var emailBody = _emailTemplateService.GetOrderCancelledEmail(user, id, orderLink);
                await _emailSender.SendEmailAsync(user.Email, "Your Order Has Been Cancelled", emailBody);

                TempData["Success"] = $"Order #{id} cancelled successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Orders));
        }

        #endregion

        #region Static Pages

        /// <summary>
        /// Displays the payments page.
        /// </summary>
        public IActionResult Payments()
        {
            return View();
        }

        /// <summary>
        /// Displays the contact us page.
        /// </summary>
        public IActionResult ContactUs()
        {
            return View();
        }

        #endregion
    }
}