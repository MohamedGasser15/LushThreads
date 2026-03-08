using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LushThreads.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        #region Fields

        private readonly IAccountService _accountService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private IEmailSender _emailSender => (IEmailSender)HttpContext.RequestServices.GetService(typeof(IEmailSender));

        private SignInManager<ApplicationUser> _signInManager => (SignInManager<ApplicationUser>)HttpContext.RequestServices.GetService(typeof(SignInManager<ApplicationUser>));

        #endregion

        #region Constructor

        public AccountController(
            IAccountService accountService,
            IEmailTemplateService emailTemplateService,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
            : base(null, userManager)
        {
            _accountService = accountService;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        #endregion

        #region Login

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage, user, requiresTwoFactor) = await _accountService.ValidateLoginAsync(model.Email, model.Password);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return View(model);
            }

            if (requiresTwoFactor)
            {
                // Generate 2FA code and store in session (since service doesn't handle session)
                var code = await _accountService.InitiateTwoFactorLoginAsync(user, model.RememberMe);
                var verificationLink = Url.Action("Enter2FACode", "Account", null, HttpContext.Request.Scheme);

                // Store in session for later verification
                HttpContext.Session.SetString("2FA_User", user.Id);
                HttpContext.Session.SetString("2FA_RememberMe", model.RememberMe.ToString());

                // Send email using template
                var emailBody = _emailTemplateService.Generate2FACodeEmail(user, code, verificationLink);
                await _emailSender.SendEmailAsync(user.Email, "Login Verification Code", emailBody);

                return RedirectToAction("Enter2FACode");
            }

            // Regular login - sign in directly (service will handle)
            await _accountService.TrackUserDeviceAsync(user, HttpContext);
            await _signInManager.SignInAsync(user, model.RememberMe);
            HttpContext.Session.SetString("lang", user.PreferredLanguage ?? "en");

            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public IActionResult Enter2FACode()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify2FA(string code)
        {
            var userId = HttpContext.Session.GetString("2FA_User");
            var rememberMe = HttpContext.Session.GetString("2FA_RememberMe") == "True";

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            var (success, errorMessage) = await _accountService.VerifyTwoFactorCodeAsync(userId, code, rememberMe, HttpContext);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return View("Enter2FACode");
            }

            HttpContext.Session.Remove("2FA_User");
            HttpContext.Session.Remove("2FA_RememberMe");

            var user = await _userManager.FindByIdAsync(userId);
            HttpContext.Session.SetString("lang", user?.PreferredLanguage ?? "en");

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage, user, verificationCode) = await _accountService.RegisterUserAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }

            // Send verification email
            var verificationLink = Url.Action("VerifyEmailCode", "Account", new { userId = user.Id }, HttpContext.Request.Scheme);
            var emailBody = _emailTemplateService.GenerateEmailConfirmationEmail(user, verificationCode);
            await _emailSender.SendEmailAsync(user.Email, "Email Confirmation Code", emailBody);

            return RedirectToAction("VerifyEmailCode", new { userId = user.Id });
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmailCode(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return View("Error");

            return View(new VerifyEmailViewModel { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmailCode(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage, user) = await _accountService.ConfirmEmailAsync(model.UserId, model.Code);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }

            // Auto sign in after confirmation
            await _signInManager.SignInAsync(user, isPersistent: false);
            await _accountService.TrackUserDeviceAsync(user, HttpContext);

            return RedirectToAction("EmailConfirmationSuccess");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailCode(string email)
        {
            var (success, errorMessage, newCode) = await _accountService.ResendEmailVerificationCodeAsync(email);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("VerifyEmailCode", new { email });
            }

            var user = await _userManager.FindByEmailAsync(email);
            var verificationLink = Url.Action("VerifyEmailCode", "Account", new { userId = user.Id }, HttpContext.Request.Scheme);
            var emailBody = _emailTemplateService.GenerateEmailConfirmationEmail(user, newCode);
            await _emailSender.SendEmailAsync(user.Email, "Email Confirmation Code", emailBody);

            TempData["Message"] = "A new code has been sent to your email.";
            return RedirectToAction("VerifyEmailCode", new { userId = user.Id });
        }

        [HttpGet]
        public IActionResult EmailConfirmationSuccess()
        {
            return View();
        }

        #endregion

        #region Forgot Password

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage, resetCode, resetToken) = await _accountService.InitiateForgotPasswordAsync(model.Email, HttpContext);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }

            var resetLink = Url.Action("ResetPassword", "Account", new { email = model.Email, code = resetToken }, HttpContext.Request.Scheme);
            var user = await _userManager.FindByEmailAsync(model.Email);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceName = System.Net.Dns.GetHostName();

            var emailBody = _emailTemplateService.GenerateForgotPasswordEmail(user, ipAddress, deviceName, DateTime.UtcNow, resetCode, resetLink);
            await _emailSender.SendEmailAsync(user.Email, "Password Reset Code", emailBody);

            return RedirectToAction("VerifyResetCode", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult VerifyResetCode(string email)
        {
            return View(new VerifyCodeViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyResetCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage, email) = await _accountService.VerifyResetCodeAsync(model.Email, model.Code);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }

            return RedirectToAction("ResetPassword", new { email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendResetCode(string email)
        {
            var (success, errorMessage, newCode) = await _accountService.ResendPasswordResetCodeAsync(email);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("VerifyResetCode", new { email });
            }

            var user = await _userManager.FindByEmailAsync(email);
            var resetLink = Url.Action("ResetPassword", "Account", new { email }, HttpContext.Request.Scheme);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceName = System.Net.Dns.GetHostName();

            var emailBody = _emailTemplateService.GenerateForgotPasswordEmail(user, ipAddress, deviceName, DateTime.UtcNow, newCode, resetLink);
            await _emailSender.SendEmailAsync(user.Email, "Password Reset Code", emailBody);

            TempData["Message"] = "A new code has been sent to your email.";
            return RedirectToAction("VerifyResetCode", new { email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage) = await _accountService.ResetPasswordAsync(model.Email, model.NewPassword);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }

            return RedirectToAction("ResetPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region External Login

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            var (success, errorMessage, user, provider, callbackReturnUrl) = await _accountService.HandleExternalLoginCallbackAsync(returnUrl, remoteError);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("Login");
            }

            if (user != null)
            {
                // User already exists and signed in
                await _accountService.TrackUserDeviceAsync(user, HttpContext);
                return LocalRedirect(callbackReturnUrl);
            }

            // New user, need to complete registration
            var email = (await _signInManager.GetExternalLoginInfoAsync())?.Principal.FindFirstValue(ClaimTypes.Email);
            return RedirectToAction("ExternalLoginConfirmation", new { returnUrl, email });
        }

        [HttpGet]
        public IActionResult ExternalLoginConfirmation(string returnUrl, string email)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            return View(new ExternalLoginConfirmationViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var (success, errorMessage, user) = await _accountService.ConfirmExternalLoginAsync(model, returnUrl, HttpContext);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            return LocalRedirect(returnUrl);
        }

        #endregion

        #region Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction("Home", "Home");
        }

        #endregion

        #region Email Sender ( IEmailSender)


        #endregion
    }
}