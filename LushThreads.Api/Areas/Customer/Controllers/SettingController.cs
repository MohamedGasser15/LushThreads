using AutoMapper;
using LushThreads.Application.DTOs.Settings;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LushThreads.Api.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/customer/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingController : ControllerBase
    {
        private readonly ISettingService _settingService;
        private readonly IAdminActivityService _adminActivityService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public SettingController(
            ISettingService settingService,
            IAdminActivityService adminActivityService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _settingService = settingService;
            _adminActivityService = adminActivityService;
            _userManager = userManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Get current user settings.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserSettingsDto>> GetSettings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _settingService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            var settingsDto = _mapper.Map<UserSettingsDto>(user);
            return Ok(settingsDto);
        }

        /// <summary>
        /// Change user language.
        /// </summary>
        [HttpPost("language")]
        public async Task<IActionResult> ChangeLanguage([FromBody] ChangeLanguageRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _settingService.ChangeLanguageAsync(userId, request.PreferredLanguage);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await _adminActivityService.LogActivityAsync(
                userId,
                "ChangeLanguage",
                $"Changed language to {request.PreferredLanguage}",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Language changed successfully." });
        }

        /// <summary>
        /// Change user currency.
        /// </summary>
        [HttpPost("currency")]
        public async Task<IActionResult> ChangeCurrency([FromBody] ChangeCurrencyRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _settingService.ChangeCurrencyAsync(userId, request.Currency);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await _adminActivityService.LogActivityAsync(
                userId,
                "ChangeCurrency",
                $"Changed currency to {request.Currency}",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Currency changed successfully." });
        }

        /// <summary>
        /// Change user payment method.
        /// </summary>
        [HttpPost("payment-method")]
        public async Task<IActionResult> ChangePaymentMethod([FromBody] ChangePaymentMethodRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _settingService.ChangePaymentMethodAsync(userId, request.PaymentMethod);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await _adminActivityService.LogActivityAsync(
                userId,
                "ChangePaymentMethod",
                $"Changed payment method to {request.PaymentMethod}",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Payment method changed successfully." });
        }

        /// <summary>
        /// Change user preferred carriers.
        /// </summary>
        [HttpPost("preferred-carriers")]
        public async Task<IActionResult> ChangePreferredCarriers([FromBody] ChangePreferredCarriersRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _settingService.ChangePreferredCarriersAsync(userId, request.PreferredCarriers);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await _adminActivityService.LogActivityAsync(
                userId,
                "ChangePreferredCarriers",
                $"Changed carriers to {request.PreferredCarriers}",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Preferred carriers changed successfully." });
        }

        /// <summary>
        /// Update primary address.
        /// </summary>
        [HttpPost("address/primary")]
        public async Task<IActionResult> UpdatePrimaryAddress([FromBody] UpdateAddressRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _settingService.UpdatePrimaryAddressAsync(userId, request.Address);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await _adminActivityService.LogActivityAsync(
                userId,
                "UpdatePrimaryAddress",
                "Updated primary address",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Primary address updated successfully." });
        }

        /// <summary>
        /// Update secondary address.
        /// </summary>
        [HttpPost("address/secondary")]
        public async Task<IActionResult> UpdateSecondaryAddress([FromBody] UpdateAddressRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _settingService.UpdateSecondaryAddressAsync(userId, request.Address);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await _adminActivityService.LogActivityAsync(
                userId,
                "UpdateSecondaryAddress",
                "Updated secondary address",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Secondary address updated successfully." });
        }

        /// <summary>
        /// Swap primary and secondary addresses.
        /// </summary>
        [HttpPost("address/swap")]
        public async Task<IActionResult> SwapAddresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _settingService.SetPrimaryAddressAsync(userId);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await _adminActivityService.LogActivityAsync(
                userId,
                "SwapAddresses",
                "Swapped primary and secondary addresses",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Addresses swapped successfully." });
        }

        /// <summary>
        /// Delete secondary address.
        /// </summary>
        [HttpDelete("address/secondary")]
        public async Task<IActionResult> DeleteSecondaryAddress()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _settingService.DeleteSecondaryAddressAsync(userId);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await _adminActivityService.LogActivityAsync(
                userId,
                "DeleteSecondaryAddress",
                "Deleted secondary address",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Secondary address deleted successfully." });
        }
    }
}
