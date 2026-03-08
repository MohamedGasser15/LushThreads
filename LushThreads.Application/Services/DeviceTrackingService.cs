using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Implementation of IDeviceTrackingService.
    /// </summary>
    public class DeviceTrackingService : IDeviceTrackingService
    {
        #region Fields

        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly ISecurityActivityRepository _securityActivityRepository;
        private readonly IIpLocationService _ipLocationService;

        #endregion

        #region Constructor

        public DeviceTrackingService(
            IUserDeviceRepository userDeviceRepository,
            ISecurityActivityRepository securityActivityRepository,
            IIpLocationService ipLocationService)
        {
            _userDeviceRepository = userDeviceRepository;
            _securityActivityRepository = securityActivityRepository;
            _ipLocationService = ipLocationService;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<string> TrackUserDeviceAsync(ApplicationUser user, HttpContext httpContext, string? deviceToken)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            var ipAddress = GetClientIpAddress(httpContext);

            var deviceInfo = new
            {
                DeviceName = GetFriendlyDeviceName(userAgent),
                DeviceType = GetDeviceType(userAgent),
                IP = ipAddress,
                OS = GetOSFromUserAgent(userAgent),
                Browser = GetBrowserFromUserAgent(userAgent),
                Location = await _ipLocationService.GetLocationFromIpAsync(ipAddress)
            };

            var existingDevice = await _userDeviceRepository.GetByUserAndTokenAsync(user.Id, deviceToken);

            if (existingDevice != null)
            {
                // Update existing device
                existingDevice.LastLoginDate = DateTime.Now;
                await _userDeviceRepository.UpdateAsync(existingDevice);

                var knownDeviceActivity = new SecurityActivity
                {
                    UserId = user.Id,
                    ActivityType = "KnownDeviceLogin",
                    Description = "Login detected from a previously used device.",
                    IpAddress = ipAddress
                };
                await _securityActivityRepository.CreateAsync(knownDeviceActivity);

                return existingDevice.DeviceToken!;
            }
            else
            {
                // Create new device
                var newDevice = new UserDevice
                {
                    UserId = user.Id,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceType = deviceInfo.DeviceType,
                    IpAddress = deviceInfo.IP,
                    OS = deviceInfo.OS,
                    Browser = deviceInfo.Browser,
                    Location = deviceInfo.Location,
                    DeviceToken = Guid.NewGuid().ToString(),
                    FirstLoginDate = DateTime.Now,
                    LastLoginDate = DateTime.Now
                };

                await _userDeviceRepository.CreateAsync(newDevice);

                var newDeviceActivity = new SecurityActivity
                {
                    UserId = user.Id,
                    ActivityType = "NewDeviceLogin",
                    Description = "Login detected from a new device.",
                    IpAddress = ipAddress
                };
                await _securityActivityRepository.CreateAsync(newDeviceActivity);

                return newDevice.DeviceToken!;
            }
        }

        /// <inheritdoc />
        public string GetClientIpAddress(HttpContext httpContext)
        {
            var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrEmpty(ip))
            {
                ip = httpContext.Connection.RemoteIpAddress?.ToString();
            }
            else
            {
                ip = ip.Split(',')[0].Trim();
            }

            if (IPAddress.TryParse(ip, out var address))
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    ip = address.MapToIPv4().ToString();
                }
                else if (ip == "::1")
                {
                    ip = "127.0.0.1";
                }
            }

            return ip ?? "Unknown";
        }

        #endregion

        #region Private Methods - Device Info Extraction

        private string GetOSFromUserAgent(string userAgent)
        {
            if (userAgent.Contains("Windows")) return "Windows";
            if (userAgent.Contains("Mac")) return "MacOS";
            if (userAgent.Contains("Linux")) return "Linux";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iPhone")) return "iOS";
            return "Unknown";
        }

        private string GetBrowserFromUserAgent(string userAgent)
        {
            if (userAgent.Contains("Edg")) return "Microsoft Edge";
            if (userAgent.Contains("Chrome")) return "Google Chrome";
            if (userAgent.Contains("Firefox")) return "Mozilla Firefox";
            if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Apple Safari";
            if (userAgent.Contains("Opera") || userAgent.Contains("OPR")) return "Opera";
            return "Unknown Browser";
        }

        private string GetFriendlyDeviceName(string userAgent)
        {
            string deviceType = userAgent.Contains("Mobile") ? "Mobile" : "Desktop";

            if (userAgent.Contains("Windows NT")) deviceType = "Windows " + deviceType;
            else if (userAgent.Contains("Macintosh")) deviceType = "Mac " + deviceType;
            else if (userAgent.Contains("Linux")) deviceType = "Linux " + deviceType;

            string browser = GetBrowserFromUserAgent(userAgent);
            return $"{deviceType} ({browser})";
        }

        private string GetDeviceType(string userAgent)
        {
            if (userAgent.Contains("Mobi") || userAgent.Contains("Android")) return "Mobile";
            if (userAgent.Contains("Tablet") || userAgent.Contains("iPad")) return "Tablet";
            if (userAgent.Contains("Windows NT") || userAgent.Contains("Macintosh") || userAgent.Contains("Linux")) return "Desktop";
            if (userAgent.Contains("Xbox") || userAgent.Contains("PlayStation")) return "Gaming Console";
            return "Unknown Device";
        }

        #endregion
    }
}
