using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Auth;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service for account management operations.
    /// Implements <see cref="IAccountService"/>.
    /// </summary>
    public class AccountService : IAccountService
    {
        #region Fields

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IRepository<SecurityActivity> _securityActivityRepository;
        private readonly IRepository<UserDevice> _userDeviceRepository;
        private readonly ILogger<AccountService> _logger;

        #endregion

        #region Constructor

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            IRepository<SecurityActivity> securityActivityRepository,
            IRepository<UserDevice> userDeviceRepository,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _securityActivityRepository = securityActivityRepository;
            _userDeviceRepository = userDeviceRepository;
            _logger = logger;
        }

        #endregion

        #region Private Helpers

        private bool SecureEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            var result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }

        private string GenerateRandomCode() => new Random().Next(100000, 999999).ToString();

        private string GetClientIpAddress(HttpContext httpContext) =>
            httpContext.Connection.RemoteIpAddress?.ToString();

        private string GetDeviceName() => System.Net.Dns.GetHostName();

        private async Task<bool> AddOrUpdateClaimAsync(ApplicationUser user, string claimType, string claimValue)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var existingClaim = claims.FirstOrDefault(c => c.Type == claimType);
            if (existingClaim != null)
                await _userManager.RemoveClaimAsync(user, existingClaim);
            return (await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue))).Succeeded;
        }

        #endregion

        #region Login & 2FA

        public async Task<(bool success, string errorMessage, ApplicationUser user, bool requiresTwoFactor)> ValidateLoginAsync(string email, string password)
        {
            _logger.LogDebug("Validating login for email {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Invalid email or password.", null, false);

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
                return (false, "Invalid email or password.", null, false);

            var requiresTwoFactor = await _userManager.GetTwoFactorEnabledAsync(user);
            return (true, null, user, requiresTwoFactor);
        }

        public async Task<string> InitiateTwoFactorLoginAsync(ApplicationUser user, bool rememberMe)
        {
            _logger.LogInformation("Initiating 2FA login for user {UserId}", user.Id);

            var code = GenerateRandomCode();
            var verificationLink = Generate2FALink(user, code); // نحتاج إلى URL، لكننا سنمررها من الـ Controller

            // Store code and rememberMe in user claims or session (will be handled by controller with session)
            await AddOrUpdateClaimAsync(user, "2FA_Code", code);
            await AddOrUpdateClaimAsync(user, "2FA_RememberMe", rememberMe.ToString());

            var emailBody = _emailTemplateService.Generate2FACodeEmail(user, code, verificationLink);
            await _emailSender.SendEmailAsync(user.Email, "Login Verification Code", emailBody);

            return code;
        }

        public async Task<(bool success, string errorMessage)> VerifyTwoFactorCodeAsync(string userId, string code, bool rememberMe, HttpContext httpContext)
        {
            _logger.LogInformation("Verifying 2FA code for user {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "User not found.");

            var claims = await _userManager.GetClaimsAsync(user);
            var storedCode = claims.FirstOrDefault(c => c.Type == "2FA_Code")?.Value;

            if (!SecureEquals(code, storedCode))
                return (false, "Invalid verification code.");

            // Sign in user
            await _signInManager.SignInAsync(user, rememberMe);
            await TrackUserDeviceAsync(user, httpContext);

            // Clean up claims
            await _userManager.RemoveClaimAsync(user, claims.First(c => c.Type == "2FA_Code"));
            var rememberMeClaim = claims.FirstOrDefault(c => c.Type == "2FA_RememberMe");
            if (rememberMeClaim != null)
                await _userManager.RemoveClaimAsync(user, rememberMeClaim);

            return (true, null);
        }

        #endregion

        #region Registration

        public async Task<(bool success, string errorMessage, ApplicationUser user, string verificationCode)> RegisterUserAsync(RegisterViewModel model)
        {
            _logger.LogInformation("Registering new user with email {Email}", model.Email);

            // Validate name
            if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Length < 3)
                return (false, "Name must be at least 3 characters long.", null, null);

            if (!Regex.IsMatch(model.Name, @"^[a-zA-Z\s]+$"))
                return (false, "Name can only contain letters and spaces.", null, null);

            // Check uniqueness
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
                return (false, "This email is already registered.", null, null);

            var existingUserByName = await _userManager.FindByNameAsync(model.Name);
            if (existingUserByName != null)
                return (false, "This name is already taken.", null, null);

            // Create user
            var user = new ApplicationUser
            {
                UserName = model.Name,
                Email = model.Email,
                Name = model.Name
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
                return (false, string.Join(", ", createResult.Errors.Select(e => e.Description)), null, null);

            // Add to default role
            await _userManager.AddToRoleAsync(user, SD.User);

            // Generate verification code
            var verificationCode = GenerateRandomCode();
            await AddOrUpdateClaimAsync(user, "EmailVerificationCode", verificationCode);

            return (true, null, user, verificationCode);
        }

        public async Task<(bool success, string errorMessage, ApplicationUser user)> ConfirmEmailAsync(string userId, string code)
        {
            _logger.LogInformation("Confirming email for user {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "User not found.", null);

            var claims = await _userManager.GetClaimsAsync(user);
            var storedCode = claims.FirstOrDefault(c => c.Type == "EmailVerificationCode")?.Value;

            if (storedCode == null || storedCode != code)
                return (false, "Invalid or expired code.", null);

            // Confirm email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return (false, "Failed to confirm email.", null);

            // Clean up
            await _userManager.RemoveClaimAsync(user, claims.First(c => c.Type == "EmailVerificationCode"));

            return (true, null, user);
        }

        public async Task<(bool success, string errorMessage, string newCode)> ResendEmailVerificationCodeAsync(string email)
        {
            _logger.LogInformation("Resending email verification code to {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "User not found.", null);

            // Check cooldown (60 seconds)
            var claims = await _userManager.GetClaimsAsync(user);
            var lastRequestClaim = claims.FirstOrDefault(c => c.Type == "LastResendTime");
            var lastRequestTime = lastRequestClaim != null ? DateTime.Parse(lastRequestClaim.Value) : DateTime.MinValue;

            if ((DateTime.UtcNow - lastRequestTime).TotalSeconds < 60)
                return (false, "Please wait before requesting a new code.", null);

            // Generate new code
            var newCode = GenerateRandomCode();
            await AddOrUpdateClaimAsync(user, "EmailVerificationCode", newCode);
            await AddOrUpdateClaimAsync(user, "LastResendTime", DateTime.UtcNow.ToString());

            return (true, null, newCode);
        }

        #endregion

        #region Password Management

        public async Task<(bool success, string errorMessage, string resetCode, string resetToken)> InitiateForgotPasswordAsync(string email, HttpContext httpContext)
        {
            _logger.LogInformation("Initiating forgot password for {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "No account found with this email.", null, null);

            var resetCode = GenerateRandomCode();
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            await AddOrUpdateClaimAsync(user, "ResetCode", resetCode);

            var ipAddress = GetClientIpAddress(httpContext);
            var deviceName = GetDeviceName();
            var requestTime = DateTime.UtcNow;

            // Generate password reset link (will be built in controller with UrlHelper)
            var resetLink = string.Empty; // سنمرره من الـ Controller

            var emailBody = _emailTemplateService.GenerateForgotPasswordEmail(user, ipAddress, deviceName, requestTime, resetCode, resetLink);
            await _emailSender.SendEmailAsync(user.Email, "Password Reset Code", emailBody);

            return (true, null, resetCode, resetToken);
        }

        public async Task<(bool success, string errorMessage, string email)> VerifyResetCodeAsync(string email, string code)
        {
            _logger.LogDebug("Verifying reset code for {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Invalid request.", null);

            var claims = await _userManager.GetClaimsAsync(user);
            var storedCode = claims.FirstOrDefault(c => c.Type == "ResetCode")?.Value;

            if (storedCode == null || storedCode != code)
                return (false, "Invalid or expired code.", null);

            return (true, null, user.Email);
        }

        public async Task<(bool success, string errorMessage)> ResetPasswordAsync(string email, string newPassword)
        {
            _logger.LogInformation("Resetting password for {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "User not found.");

            var result = await _userManager.RemovePasswordAsync(user);
            if (!result.Succeeded)
                return (false, "Failed to remove old password.");

            result = await _userManager.AddPasswordAsync(user, newPassword);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            // Log security activity
            var activity = new SecurityActivity
            {
                UserId = user.Id,
                ActivityType = "PasswordChange",
                Description = "Password changed via forgot password",
                IpAddress = null // يمكن تمريرها من الـ Controller
            };
            await _securityActivityRepository.CreateAsync(activity);

            // Clean up claims
            var claims = await _userManager.GetClaimsAsync(user);
            var resetCodeClaim = claims.FirstOrDefault(c => c.Type == "ResetCode");
            if (resetCodeClaim != null)
                await _userManager.RemoveClaimAsync(user, resetCodeClaim);
            var lastResendClaim = claims.FirstOrDefault(c => c.Type == "LastResendTime");
            if (lastResendClaim != null)
                await _userManager.RemoveClaimAsync(user, lastResendClaim);

            return (true, null);
        }

        public async Task<(bool success, string errorMessage, string newCode)> ResendPasswordResetCodeAsync(string email)
        {
            _logger.LogInformation("Resending password reset code to {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "User not found.", null);

            // Check cooldown
            var claims = await _userManager.GetClaimsAsync(user);
            var lastRequestClaim = claims.FirstOrDefault(c => c.Type == "LastResendTime");
            var lastRequestTime = lastRequestClaim != null ? DateTime.Parse(lastRequestClaim.Value) : DateTime.MinValue;

            if ((DateTime.UtcNow - lastRequestTime).TotalSeconds < 60)
                return (false, "Please wait before requesting a new code.", null);

            var newCode = GenerateRandomCode();
            await AddOrUpdateClaimAsync(user, "ResetCode", newCode);
            await AddOrUpdateClaimAsync(user, "LastResendTime", DateTime.UtcNow.ToString());

            return (true, null, newCode);
        }

        #endregion

        #region External Login

        public async Task<(bool success, string errorMessage, ApplicationUser user, string provider, string returnUrl)> HandleExternalLoginCallbackAsync(string returnUrl, string remoteError)
        {
            if (remoteError != null)
                return (false, $"Error from {remoteError} provider.", null, null, null);

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return (false, "Unable to retrieve login information.", null, null, null);

            // Check if user already exists with this external login
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
                if (result.Succeeded)
                {
                    return (true, null, user, info.LoginProvider, returnUrl);
                }
            }

            // New user, need confirmation
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            return (true, null, null, info.LoginProvider, returnUrl);
        }

        public async Task<(bool success, string errorMessage, ApplicationUser user)> ConfirmExternalLoginAsync(ExternalLoginConfirmationViewModel model, string returnUrl, HttpContext httpContext)
        {
            _logger.LogInformation("Confirming external login for email {Email}", model.Email);

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return (false, "External login information not found.", null);

            // Check if email is already registered
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return (false, "Email already registered.", null);

            // Create new user
            var user = new ApplicationUser
            {
                Name = model.Name,
                Email = model.Email,
                UserName = model.Email
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return (false, string.Join(", ", createResult.Errors.Select(e => e.Description)), null);

            await _userManager.AddToRoleAsync(user, SD.User);

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
                return (false, string.Join(", ", addLoginResult.Errors.Select(e => e.Description)), null);

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            await _signInManager.SignInAsync(user, isPersistent: false);
            await TrackUserDeviceAsync(user, httpContext);
            await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

            return (true, null, user);
        }

        #endregion

        #region Utilities

        public async Task TrackUserDeviceAsync(ApplicationUser user, HttpContext httpContext)
        {
            var deviceToken = httpContext.Request.Cookies["DeviceToken"];
            if (string.IsNullOrEmpty(deviceToken))
            {
                deviceToken = Guid.NewGuid().ToString();
                httpContext.Response.Cookies.Append("DeviceToken", deviceToken, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
            }

            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            var ipAddress = GetClientIpAddress(httpContext);

            var existingDevice = await _userDeviceRepository.GetAsync(d => d.DeviceToken == deviceToken);
            if (existingDevice != null)
            {
                existingDevice.LastLoginDate = DateTime.Now;
                await _userDeviceRepository.UpdateAsync(existingDevice);
            }
            else
            {
                var newDevice = new UserDevice
                {
                    UserId = user.Id,
                    DeviceToken = deviceToken,
                    DeviceName = GetDeviceName(),
                    Browser = userAgent,
                    IpAddress = ipAddress,
                    LastLoginDate = DateTime.Now
                };
                await _userDeviceRepository.CreateAsync(newDevice);
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        #endregion

        #region Private Link Generators (will be overridden in controller with UrlHelper)

        private string Generate2FALink(ApplicationUser user, string code)
        {
            // This should be generated in controller with UrlHelper
            return "#";
        }

        private string GenerateVerificationLink(ApplicationUser user, string code)
        {
            return "#";
        }

        private string GeneratePasswordResetLink(ApplicationUser user, string token)
        {
            return "#";
        }

        #endregion
    }
}