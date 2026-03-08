using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Service interface for account-related operations.
    /// </summary>
    public interface IAccountService
    {
        #region Login & 2FA

        /// <summary>
        /// Validates user credentials for login.
        /// </summary>
        Task<(bool success, string errorMessage, ApplicationUser user, bool requiresTwoFactor)> ValidateLoginAsync(string email, string password);

        /// <summary>
        /// Initiates 2FA login process by sending verification code.
        /// </summary>
        Task<string> InitiateTwoFactorLoginAsync(ApplicationUser user, bool rememberMe);

        /// <summary>
        /// Verifies 2FA code and signs in the user.
        /// </summary>
        Task<(bool success, string errorMessage)> VerifyTwoFactorCodeAsync(string userId, string code, bool rememberMe, HttpContext httpContext);

        #endregion

        #region Registration

        /// <summary>
        /// Registers a new user.
        /// </summary>
        Task<(bool success, string errorMessage, ApplicationUser user, string verificationCode)> RegisterUserAsync(RegisterViewModel model);

        /// <summary>
        /// Confirms email using verification code.
        /// </summary>
        Task<(bool success, string errorMessage, ApplicationUser user)> ConfirmEmailAsync(string userId, string code);

        /// <summary>
        /// Resends email verification code.
        /// </summary>
        Task<(bool success, string errorMessage, string newCode)> ResendEmailVerificationCodeAsync(string email);

        #endregion

        #region Password Management

        /// <summary>
        /// Initiates forgot password process by sending reset code.
        /// </summary>
        Task<(bool success, string errorMessage, string resetCode, string resetToken)> InitiateForgotPasswordAsync(string email, HttpContext httpContext);

        /// <summary>
        /// Verifies password reset code.
        /// </summary>
        Task<(bool success, string errorMessage, string email)> VerifyResetCodeAsync(string email, string code);

        /// <summary>
        /// Resets password using verified code.
        /// </summary>
        Task<(bool success, string errorMessage)> ResetPasswordAsync(string email, string newPassword);

        /// <summary>
        /// Resends password reset code.
        /// </summary>
        Task<(bool success, string errorMessage, string newCode)> ResendPasswordResetCodeAsync(string email);

        #endregion

        #region External Login

        /// <summary>
        /// Handles external login callback.
        /// </summary>
        Task<(bool success, string errorMessage, ApplicationUser user, string provider, string returnUrl)> HandleExternalLoginCallbackAsync(string returnUrl, string remoteError);

        /// <summary>
        /// Confirms external login and creates local user if needed.
        /// </summary>
        Task<(bool success, string errorMessage, ApplicationUser user)> ConfirmExternalLoginAsync(ExternalLoginConfirmationViewModel model, string returnUrl, HttpContext httpContext);

        #endregion

        #region Utilities

        /// <summary>
        /// Tracks user device after successful login.
        /// </summary>
        Task TrackUserDeviceAsync(ApplicationUser user, HttpContext httpContext);

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        Task LogoutAsync();

        #endregion
    }
}
