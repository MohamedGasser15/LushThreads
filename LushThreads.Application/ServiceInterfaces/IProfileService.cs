using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using LushThreads.Domain.ViewModels.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Service interface for user profile and security operations.
    /// </summary>
    public interface IProfileService
    {
        #region Profile Management

        /// <summary>
        /// Gets the profile view model for the specified user.
        /// </summary>
        Task<ProfileViewModel> GetProfileAsync(string userId);

        /// <summary>
        /// Updates the user's name.
        /// </summary>
        Task UpdateNameAsync(string userId, string name);

        /// <summary>
        /// Updates the user's email.
        /// </summary>
        Task UpdateEmailAsync(string userId, string newEmail);

        /// <summary>
        /// Updates the user's phone number.
        /// </summary>
        Task UpdatePhoneAsync(string userId, string phoneNumber);

        /// <summary>
        /// Updates the user's address.
        /// </summary>
        Task UpdateAddressAsync(string userId, string address);

        /// <summary>
        /// Updates the user's postal code.
        /// </summary>
        Task UpdatePostalCodeAsync(string userId, string postalCode);

        /// <summary>
        /// Updates the user's country.
        /// </summary>
        Task UpdateCountryAsync(string userId, string country);

        #endregion

        #region Security & 2FA

        /// <summary>
        /// Gets the security view model for the specified user.
        /// </summary>
        Task<ProfileViewModel> GetSecurityViewModelAsync(string userId);

        /// <summary>
        /// Generates and sends email verification code.
        /// </summary>
        Task<string> GenerateEmailVerificationCodeAsync(string userId);

        /// <summary>
        /// Verifies email confirmation code.
        /// </summary>
        Task<bool> VerifyEmailCodeAsync(string userId, string code, string storedCode);

        /// <summary>
        /// Changes user password and logs activity.
        /// </summary>
        Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, string ipAddress);

        /// <summary>
        /// Initiates 2FA setup by sending verification code.
        /// </summary>
        Task<string> Initiate2FASetupAsync(string userId);

        /// <summary>
        /// Verifies 2FA code and enables two-factor authentication.
        /// </summary>
        Task<bool> VerifyAndEnable2FAAsync(string userId, string code, string storedCode, DateTime expiry);

        /// <summary>
        /// Disables two-factor authentication.
        /// </summary>
        Task Disable2FAAsync(string userId);

        #endregion

        #region Devices

        /// <summary>
        /// Gets all devices for the user.
        /// </summary>
        Task<IEnumerable<UserDevice>> GetUserDevicesAsync(string userId);

        /// <summary>
        /// Removes a specific device.
        /// </summary>
        Task RemoveDeviceAsync(string userId, Guid deviceId, string currentDeviceToken);

        /// <summary>
        /// Removes inactive devices older than 30 days.
        /// </summary>
        Task<int> RemoveInactiveDevicesAsync(string userId, string currentDeviceToken);

        #endregion

        #region Orders

        /// <summary>
        /// Gets paginated orders for the user.
        /// </summary>
        Task<(IEnumerable<OrderVM> Orders, int TotalCount)> GetUserOrdersAsync(string userId, int page, int pageSize);

        /// <summary>
        /// Cancels an order by the user.
        /// </summary>
        Task CancelOrderAsync(string userId, int orderId, string ipAddress);

        #endregion

        #region Utilities

        /// <summary>
        /// Validates email format.
        /// </summary>
        bool IsValidEmail(string email);

        /// <summary>
        /// Generates a secure 6-digit code.
        /// </summary>
        string GenerateSecureCode();

        #endregion
    }
}
