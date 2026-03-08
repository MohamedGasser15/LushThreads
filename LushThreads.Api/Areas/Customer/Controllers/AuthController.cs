using AutoMapper;
using LushThreads.Application.DTOs.Auth;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LushThreads.Api.Areas.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAccountService accountService,
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            IMapper mapper,
            ILogger<AuthController> logger)
        {
            _accountService = accountService;
            _tokenService = tokenService;
            _userManager = userManager;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _mapper = mapper;
            _logger = logger;
        }

        #region Login & 2FA (API version)

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // استخدام الدالة الجديدة ApiValidateLoginAsync
            var (success, errorMessage, user, requiresTwoFactor) = await _accountService.ApiValidateLoginAsync(request.Email, request.Password);
            if (!success)
                return Unauthorized(new { message = errorMessage });

            if (requiresTwoFactor)
            {
                // استخدام الدالة الموجودة (لا تحتاج تغيير)
                var code = await _accountService.InitiateTwoFactorLoginAsync(user, request.RememberMe);
                return Ok(new LoginResponseDto
                {
                    RequiresTwoFactor = true,
                    UserId = user.Id
                });
            }

            // تسجيل الدخول المباشر - إنشاء توكن
            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenService.CreateTokenAsync(user, roles);

            return Ok(new LoginResponseDto
            {
                RequiresTwoFactor = false,
                Token = token,
                Email = user.Email,
                Name = user.Name
            });
        }

        [HttpPost("verify-2fa")]
        public async Task<ActionResult<LoginResponseDto>> VerifyTwoFactor([FromBody] TwoFactorVerifyDto request)
        {
            // استخدام الدالة الجديدة ApiVerifyTwoFactorAsync
            var (success, errorMessage, user) = await _accountService.ApiVerifyTwoFactorAsync(request.UserId, request.Code, request.RememberMe, HttpContext);
            if (!success)
                return BadRequest(new { message = errorMessage });

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenService.CreateTokenAsync(user, roles);

            return Ok(new LoginResponseDto
            {
                RequiresTwoFactor = false,
                Token = token,
                Email = user.Email,
                Name = user.Name
            });
        }

        #endregion

        #region Registration & Email Confirmation

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO إلى ViewModel
            var registerVm = _mapper.Map<RegisterViewModel>(request);

            // استخدام الدالة الموجودة (لا تحتاج تغيير)
            var (success, errorMessage, user, verificationCode) = await _accountService.RegisterUserAsync(registerVm);
            if (!success)
                return BadRequest(new { message = errorMessage });

            // إرسال كود التفعيل
            var verificationLink = Url.Action(nameof(VerifyEmail), "Auth", new { userId = user.Id, code = verificationCode }, Request.Scheme);
            var emailBody = _emailTemplateService.GenerateEmailConfirmationEmail(user, verificationCode);
            await _emailSender.SendEmailAsync(user.Email, "Email Confirmation Code", emailBody);

            return Ok(new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Message = "Registration successful. Please verify your email."
            });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            // Map DTO إلى ViewModel (اختياري، يمكن تمرير الخصائص مباشرة)
            var (success, errorMessage, user) = await _accountService.ConfirmEmailAsync(request.UserId, request.Code);
            if (!success)
                return BadRequest(new { message = errorMessage });

            return Ok(new { message = "Email verified successfully." });
        }

        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationRequestDto request)
        {
            var (success, errorMessage, newCode) = await _accountService.ResendEmailVerificationCodeAsync(request.Email);
            if (!success)
                return BadRequest(new { message = errorMessage });

            var user = await _userManager.FindByEmailAsync(request.Email);
            var verificationLink = Url.Action(nameof(VerifyEmail), "Auth", new { userId = user.Id, code = newCode }, Request.Scheme);
            var emailBody = _emailTemplateService.GenerateEmailConfirmationEmail(user, newCode);
            await _emailSender.SendEmailAsync(user.Email, "Email Confirmation Code", emailBody);

            return Ok(new { message = "Verification email resent." });
        }

        #endregion

        #region Password Management

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var (success, errorMessage, resetCode, resetToken) = await _accountService.InitiateForgotPasswordAsync(request.Email, HttpContext);
            if (!success)
                return BadRequest(new { message = errorMessage });

            var user = await _userManager.FindByEmailAsync(request.Email);
            var resetLink = Url.Action(nameof(ResetPassword), "Auth", new { email = request.Email, token = resetToken }, Request.Scheme);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceName = System.Net.Dns.GetHostName();

            var emailBody = _emailTemplateService.GenerateForgotPasswordEmail(user, ipAddress, deviceName, DateTime.UtcNow, resetCode, resetLink);
            await _emailSender.SendEmailAsync(user.Email, "Password Reset Code", emailBody);

            return Ok(new { message = "Reset code sent to email." });
        }

        [HttpPost("verify-reset-code")]
        public async Task<ActionResult<VerifyResetCodeResponseDto>> VerifyResetCode([FromBody] VerifyResetCodeRequestDto request)
        {
            var (success, errorMessage, email) = await _accountService.VerifyResetCodeAsync(request.Email, request.Code);
            if (!success)
                return BadRequest(new { message = errorMessage });

            return Ok(new VerifyResetCodeResponseDto
            {
                Email = email,
                IsValid = true
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var (success, errorMessage) = await _accountService.ResetPasswordAsync(request.Email, request.NewPassword);
            if (!success)
                return BadRequest(new { message = errorMessage });

            return Ok(new { message = "Password reset successfully." });
        }

        #endregion

        #region External Login (اختياري - يمكن تطويره لاحقاً)

        [HttpPost("external-login")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // يتطلب استخدام SignInManager، قد لا يكون مناسباً للـ API
            return BadRequest("External login is not supported via API yet.");
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            return BadRequest("External login callback is not supported via API yet.");
        }

        #endregion

        #region Logout

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return Ok(new { message = "Logged out successfully." });
        }

        #endregion
    }
}