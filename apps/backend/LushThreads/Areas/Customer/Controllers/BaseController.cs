using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LushThreads.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BaseController : Controller
    {
        #region Fields

        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IDeviceTrackingService _deviceTrackingService;

        #endregion

        #region Constructor

        public BaseController(
            UserManager<ApplicationUser> userManager,
            IDeviceTrackingService deviceTrackingService)
        {
            _userManager = userManager;
            _deviceTrackingService = deviceTrackingService;
        }

        #endregion

        #region Public Methods (Helpers)

        /// <summary>
        /// Validates email format and checks for common TLDs.
        /// </summary>
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                string[] parts = mailAddress.Host.Split('.');
                if (parts.Length < 2 || parts.Any(string.IsNullOrWhiteSpace))
                    return false;

                string[] commonTlds = { "com", "net", "org", "edu", "gov", "io", "co", "uk", "de", "fr" };
                string lastPart = parts.Last().ToLower();

                if (lastPart.Length < 2 || lastPart.Length > 6 || (!commonTlds.Contains(lastPart) && lastPart.Any(c => !char.IsLetter(c))))
                    return false;

                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tracks user device information and logs login activity using the device tracking service.
        /// </summary>
        public async Task TrackUserDevice(ApplicationUser user)
        {
            var deviceToken = Request.Cookies["DeviceToken"];
            var newToken = await _deviceTrackingService.TrackUserDeviceAsync(user, HttpContext, deviceToken);

            // Update cookie if a new token was generated
            if (newToken != deviceToken)
            {
                Response.Cookies.Append("DeviceToken", newToken, new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
            }
        }

        /// <summary>
        /// Retrieves client IP address using the device tracking service.
        /// </summary>
        public string GetClientIpAddress()
        {
            return _deviceTrackingService.GetClientIpAddress(HttpContext);
        }

        #endregion
    }
}