using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Service interface for managing user settings and preferences.
    /// </summary>
    public interface ISettingService
    {
        #region Settings Management

        /// <summary>
        /// Gets the current user by their ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        Task<ApplicationUser?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Changes the user's preferred language.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="preferredLanguage">The new language code.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> ChangeLanguageAsync(string userId, string preferredLanguage);

        /// <summary>
        /// Changes the user's preferred currency.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="currency">The new currency code.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> ChangeCurrencyAsync(string userId, string currency);

        /// <summary>
        /// Changes the user's preferred payment method.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="paymentMethod">The new payment method.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> ChangePaymentMethodAsync(string userId, string paymentMethod);

        /// <summary>
        /// Changes the user's preferred carriers.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="preferredCarriers">The new carriers.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> ChangePreferredCarriersAsync(string userId, string preferredCarriers);

        #endregion

        #region Address Management

        /// <summary>
        /// Updates the user's primary address.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="address">The new primary address.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> UpdatePrimaryAddressAsync(string userId, string address);

        /// <summary>
        /// Updates the user's secondary address.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="address">The new secondary address.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> UpdateSecondaryAddressAsync(string userId, string address);

        /// <summary>
        /// Swaps the primary and secondary addresses.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> SetPrimaryAddressAsync(string userId);

        /// <summary>
        /// Deletes the secondary address.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>IdentityResult indicating success or failure.</returns>
        Task<IdentityResult> DeleteSecondaryAddressAsync(string userId);

        #endregion
    }
}
