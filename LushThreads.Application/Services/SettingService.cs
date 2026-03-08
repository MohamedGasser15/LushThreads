using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service implementation for managing user settings and preferences.
    /// </summary>
    public class SettingService : ISettingService
    {
        #region Fields

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SettingService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingService"/> class.
        /// </summary>
        /// <param name="userManager">Identity user manager.</param>
        /// <param name="adminActivityService">Service for logging admin activities.</param>
        /// <param name="logger">Logger instance.</param>
        public SettingService(
            UserManager<ApplicationUser> userManager,
            ILogger<SettingService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates a user property and logs the activity.
        /// </summary>
        private async Task<IdentityResult> UpdateUserPropertyAsync(ApplicationUser user, Action<ApplicationUser> updateAction, string activityType, string description)
        {
            try
            {
                updateAction(user);
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("{ActivityType} succeeded for user {UserId}.", activityType, user.Id);
                    // Note: We don't have IP address here; will be passed from controller
                }
                else
                {
                    _logger.LogWarning("{ActivityType} failed for user {UserId}. Errors: {Errors}", activityType, user.Id, string.Join(", ", result.Errors));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during {ActivityType} for user {UserId}.", activityType, user.Id);
                throw;
            }
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            _logger.LogDebug("Retrieving user with ID {UserId}.", userId);
            return await _userManager.FindByIdAsync(userId);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ChangeLanguageAsync(string userId, string preferredLanguage)
        {
            _logger.LogInformation("Changing language for user {UserId} to {Language}.", userId, preferredLanguage);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for language change.", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await UpdateUserPropertyAsync(user,
                u => u.PreferredLanguage = preferredLanguage,
                "ChangeLanguage",
                $"Changed language to {preferredLanguage}");

            return result;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ChangeCurrencyAsync(string userId, string currency)
        {
            _logger.LogInformation("Changing currency for user {UserId} to {Currency}.", userId, currency);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for currency change.", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await UpdateUserPropertyAsync(user,
                u => u.Currency = currency,
                "ChangeCurrency",
                $"Changed currency to {currency}");

            return result;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ChangePaymentMethodAsync(string userId, string paymentMethod)
        {
            _logger.LogInformation("Changing payment method for user {UserId} to {PaymentMethod}.", userId, paymentMethod);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for payment method change.", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await UpdateUserPropertyAsync(user,
                u => u.PaymentMehtod = paymentMethod, // Note: property typo kept as original
                "ChangePaymentMethod",
                $"Changed payment method to {paymentMethod}");

            return result;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ChangePreferredCarriersAsync(string userId, string preferredCarriers)
        {
            _logger.LogInformation("Changing preferred carriers for user {UserId} to {Carriers}.", userId, preferredCarriers);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for preferred carriers change.", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await UpdateUserPropertyAsync(user,
                u => u.PreferredCarriers = preferredCarriers,
                "ChangePreferredCarriers",
                $"Changed preferred carriers to {preferredCarriers}");

            return result;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> UpdatePrimaryAddressAsync(string userId, string address)
        {
            _logger.LogInformation("Updating primary address for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for primary address update.", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await UpdateUserPropertyAsync(user,
                u =>
                {
                    u.StreetAddress = address;
                    u.SelectedAddress = "primary";
                },
                "UpdatePrimaryAddress",
                $"Updated primary address to '{address}'");

            return result;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> UpdateSecondaryAddressAsync(string userId, string address)
        {
            _logger.LogInformation("Updating secondary address for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for secondary address update.", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await UpdateUserPropertyAsync(user,
                u => u.StreetAddress2 = address,
                "UpdateSecondaryAddress",
                $"Updated secondary address to '{address}'");

            return result;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> SetPrimaryAddressAsync(string userId)
        {
            _logger.LogInformation("Swapping primary and secondary addresses for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for swapping addresses.", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            if (string.IsNullOrEmpty(user.StreetAddress2))
            {
                _logger.LogWarning("Cannot swap addresses: secondary address is empty for user {UserId}.", userId);
                return IdentityResult.Failed(new IdentityError { Description = "Secondary address is empty." });
            }

            var temp = user.StreetAddress;
            user.StreetAddress = user.StreetAddress2;
            user.StreetAddress2 = temp;
            user.SelectedAddress = "primary";

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Addresses swapped successfully for user {UserId}.", userId);
            }
            else
            {
                _logger.LogWarning("Failed to swap addresses for user {UserId}.", userId);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> DeleteSecondaryAddressAsync(string userId)
        {
            _logger.LogInformation("Deleting secondary address for user {UserId}.", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for secondary address deletion.", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            user.StreetAddress2 = null;
            if (user.SelectedAddress == "secondary")
                user.SelectedAddress = "primary";

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Secondary address deleted for user {UserId}.", userId);
            }
            else
            {
                _logger.LogWarning("Failed to delete secondary address for user {UserId}.", userId);
            }

            return result;
        }

        #endregion
    }
}
