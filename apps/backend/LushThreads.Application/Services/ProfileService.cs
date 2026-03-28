using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using LushThreads.Domain.ViewModels.Profile;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service for user profile and security operations.
    /// Implements <see cref="IProfileService"/>.
    /// </summary>
    public class ProfileService : IProfileService
    {
        #region Fields

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly ISecurityActivityRepository _securityActivityRepository;
        private readonly IAdminActivityService _adminActivityService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<ProfileService> _logger;

        #endregion

        #region Constructor

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderHeaderRepository,
            IOrderDetailRepository orderDetailRepository,
            IUserDeviceRepository userDeviceRepository,
            ISecurityActivityRepository securityActivityRepository,
            IAdminActivityService adminActivityService,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            ILogger<ProfileService> logger)
        {
            _userManager = userManager;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _userDeviceRepository = userDeviceRepository;
            _securityActivityRepository = securityActivityRepository;
            _adminActivityService = adminActivityService;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        #endregion

        #region Profile Management

        /// <inheritdoc />
        public async Task<ProfileViewModel> GetProfileAsync(string userId)
        {
            _logger.LogDebug("Retrieving profile for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            return new ProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.StreetAddress,
                PostalCode = user.PostalCode,
                Country = user.Country
            };
        }

        /// <inheritdoc />
        public async Task UpdateNameAsync(string userId, string name)
        {
            _logger.LogInformation("Updating name for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.Name = name;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        /// <inheritdoc />
        public async Task UpdateEmailAsync(string userId, string newEmail)
        {
            _logger.LogInformation("Updating email for user {UserId}.", userId);

            if (!IsValidEmail(newEmail))
                throw new InvalidOperationException("Invalid email address.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null && existingUser.Id != user.Id)
                throw new InvalidOperationException("Email address already in use.");

            user.Email = newEmail;
            user.EmailConfirmed = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        /// <inheritdoc />
        public async Task UpdatePhoneAsync(string userId, string phoneNumber)
        {
            _logger.LogInformation("Updating phone for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.PhoneNumber = phoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        /// <inheritdoc />
        public async Task UpdateAddressAsync(string userId, string address)
        {
            _logger.LogInformation("Updating address for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.StreetAddress = address;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        /// <inheritdoc />
        public async Task UpdatePostalCodeAsync(string userId, string postalCode)
        {
            _logger.LogInformation("Updating postal code for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.PostalCode = postalCode;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        /// <inheritdoc />
        public async Task UpdateCountryAsync(string userId, string country)
        {
            _logger.LogInformation("Updating country for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.Country = country;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        #endregion

        #region Security & 2FA

        /// <inheritdoc />
        public async Task<ProfileViewModel> GetSecurityViewModelAsync(string userId)
        {
            _logger.LogDebug("Retrieving security view model for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var devices = await _userDeviceRepository.GetAllAsync(
                filter: d => d.UserId == userId,
                orderBy: q => q.OrderByDescending(d => d.LastLoginDate),
                take: 2
            );

            var activities = await _securityActivityRepository.GetAllAsync(
                filter: a => a.UserId == userId,
                orderBy: q => q.OrderByDescending(a => a.ActivityDate),
                take: 5
            );

            return new ProfileViewModel
            {
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                IsTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                ConnectedDevices = devices.ToList(),
                RecentSecurityActivities = activities.ToList()
            };
        }

        /// <inheritdoc />
        public async Task<string> GenerateEmailVerificationCodeAsync(string userId)
        {
            _logger.LogInformation("Generating email verification code for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var code = GenerateSecureCode();
            var emailBody = _emailTemplateService.GenerateEmailConfirmationEmail(user, code);
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email", emailBody);

            return code;
        }

        /// <inheritdoc />
        public Task<bool> VerifyEmailCodeAsync(string userId, string code, string storedCode)
        {
            _logger.LogInformation("Verifying email code for user {UserId}.", userId);

            var isValid = !string.IsNullOrEmpty(storedCode) && storedCode == code;
            return Task.FromResult(isValid);
        }

        /// <inheritdoc />
        public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, string ipAddress)
        {
            _logger.LogInformation("Changing password for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            // Log security activity
            var activity = new SecurityActivity
            {
                UserId = user.Id,
                ActivityType = "PasswordChange",
                Description = "Password changed",
                IpAddress = ipAddress
            };
            await _securityActivityRepository.CreateAsync(activity);

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Send confirmation email (optional, but kept from original)
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResetLink = $"#"; // We'll need UrlHelper, but can't in service. Pass from controller.
            // For now, we'll skip sending email here; it's better to handle in controller with IUrlHelper.
        }

        /// <inheritdoc />
        public async Task<string> Initiate2FASetupAsync(string userId)
        {
            _logger.LogInformation("Initiating 2FA setup for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            if (!user.EmailConfirmed)
                throw new InvalidOperationException("Please confirm your email first.");

            var code = GenerateSecureCode();
            var emailBody = _emailTemplateService.Generate2FASetupEmail(user, code);
            await _emailSender.SendEmailAsync(user.Email, "Enable 2FA - Verification Code", emailBody);

            return code;
        }

        /// <inheritdoc />
        public Task<bool> VerifyAndEnable2FAAsync(string userId, string code, string storedCode, DateTime expiry)
        {
            _logger.LogInformation("Verifying 2FA code for user {UserId}.", userId);

            if (DateTime.Now > expiry)
                return Task.FromResult(false);

            var isValid = string.Equals(code?.Trim(), storedCode, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(isValid);
        }

        /// <inheritdoc />
        public async Task Disable2FAAsync(string userId)
        {
            _logger.LogInformation("Disabling 2FA for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to disable 2FA.");
        }

        #endregion

        #region Devices

        /// <inheritdoc />
        public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(string userId)
        {
            _logger.LogDebug("Retrieving devices for user {UserId}.", userId);

            return await _userDeviceRepository.GetAllAsync(
                filter: d => d.UserId == userId,
                orderBy: q => q.OrderByDescending(d => d.LastLoginDate)
            );
        }

        /// <inheritdoc />
        public async Task RemoveDeviceAsync(string userId, Guid deviceId, string currentDeviceToken)
        {
            _logger.LogInformation("Removing device {DeviceId} for user {UserId}.", deviceId, userId);

            var device = await _userDeviceRepository.GetAsync(d => d.Id == deviceId && d.UserId == userId);
            if (device == null)
                throw new InvalidOperationException("Device not found.");

            // Invalidate security stamp if removing current device? In original, they call UpdateSecurityStampAsync when removing any device.
            // We'll do it for any removal.
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.UpdateSecurityStampAsync(user);

            await _userDeviceRepository.DeleteAsync(device);
        }

        /// <inheritdoc />
        public async Task<int> RemoveInactiveDevicesAsync(string userId, string currentDeviceToken)
        {
            _logger.LogInformation("Removing inactive devices for user {UserId}.", userId);

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var inactiveDevices = await _userDeviceRepository.GetAllAsync(
                filter: d => d.UserId == userId &&
                             d.LastLoginDate < thirtyDaysAgo &&
                             d.DeviceToken != currentDeviceToken
            );

            if (inactiveDevices.Any())
            {
                var user = await _userManager.FindByIdAsync(userId);
                await _userManager.UpdateSecurityStampAsync(user);
                await _userDeviceRepository.DeleteRangeAsync(inactiveDevices);
            }

            return inactiveDevices.Count();
        }

        #endregion

        #region Orders

        /// <inheritdoc />
        public async Task<(IEnumerable<OrderVM> Orders, int TotalCount)> GetUserOrdersAsync(string userId, int page, int pageSize)
        {
            _logger.LogDebug("Retrieving orders for user {UserId}, page {Page}.", userId, page);

            var totalCount = await _orderHeaderRepository.GetAllAsync(
                filter: oh => oh.ApplicationUserId == userId,
                isTracking: false
            );

            var orderHeaders = await _orderHeaderRepository.GetAllAsync(
                filter: oh => oh.ApplicationUserId == userId,
                orderBy: q => q.OrderByDescending(oh => oh.OrderDate),
                skip: (page - 1) * pageSize,
                take: pageSize,
                isTracking: false
            );

            var orderIds = orderHeaders.Select(oh => oh.Id).ToList();
            var orderDetails = await _orderDetailRepository.GetAllAsync(
                filter: od => orderIds.Contains(od.OrderHeaderId),
                includeProperties: "Product"
            );

            var orderVMs = orderHeaders.Select(oh => new OrderVM
            {
                OrderHeader = oh,
                OrderDetails = orderDetails.Where(od => od.OrderHeaderId == oh.Id).ToList()
            }).ToList();

            return (orderVMs, totalCount.Count);
        }

        /// <inheritdoc />
        public async Task CancelOrderAsync(string userId, int orderId, string ipAddress)
        {
            _logger.LogInformation("Cancelling order {OrderId} by user {UserId}.", orderId, userId);

            var order = await _orderHeaderRepository.GetAsync(
                filter: o => o.Id == orderId && o.ApplicationUserId == userId,
                includeProperties: "ApplicationUser"
            );

            if (order == null)
                throw new InvalidOperationException("Order not found or unauthorized.");

            if (order.OrderStatus != SD.StatusPending && order.OrderStatus != SD.StatusApproved)
                throw new InvalidOperationException($"Cannot cancel order with status: {order.OrderStatus}.");

            // Process refund if payment was made
            if (!string.IsNullOrEmpty(order.PaymentIntentId))
            {
                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = order.PaymentIntentId,
                    Reason = RefundReasons.RequestedByCustomer
                };

                try
                {
                    await refundService.CreateAsync(refundOptions);
                    order.PaymentStatus = SD.StatusRefunded;
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Refund failed for order {OrderId}.", orderId);
                    throw new InvalidOperationException("Refund failed. Please check Stripe dashboard.");
                }
            }

            order.OrderStatus = SD.StatusCancelled;
            await _orderHeaderRepository.UpdateAsync(order);

            // Log admin activity? This is user-initiated, maybe log as security activity.
            var activity = new SecurityActivity
            {
                UserId = userId,
                ActivityType = "OrderCancellation",
                Description = $"Cancelled Order #{orderId}",
                IpAddress = ipAddress
            };
            await _securityActivityRepository.CreateAsync(activity);
        }

        #endregion

        #region Utilities

        /// <inheritdoc />
        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public string GenerateSecureCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return (BitConverter.ToUInt32(bytes, 0) % 1000000).ToString("D6");
        }

        #endregion
    }
}