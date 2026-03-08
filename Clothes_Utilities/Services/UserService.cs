using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service for user management operations using Identity and DbContext.
    /// </summary>
    public class UserService : IUserService
    {
        #region Fields

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository<UserDevice> _userDeviceRepository; // Assuming UserDevice exists
        private readonly IAdminActivityService _adminActivityService;
        private readonly ILogger<UserService> _logger;

        #endregion

        #region Constructor

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IRepository<UserDevice> userDeviceRepository,
            IAdminActivityService adminActivityService,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userDeviceRepository = userDeviceRepository;
            _adminActivityService = adminActivityService;
            _logger = logger;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Populates the Role property for each user based on their assigned roles.
        /// </summary>
        private async Task PopulateUserRolesAsync(IEnumerable<ApplicationUser> users)
        {
            var allRoles = await _roleManager.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);
            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                user.Role = userRoles.FirstOrDefault() ?? "None";
            }
        }

        /// <summary>
        /// Gets a list of all roles as SelectListItem for dropdowns.
        /// </summary>
        private async Task<IEnumerable<SelectListItem>> GetRoleListAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id
            }).ToList();
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            _logger.LogDebug("Retrieving all users.");
            var users = await _userManager.Users.ToListAsync();
            await PopulateUserRolesAsync(users);
            _logger.LogDebug("Retrieved {Count} users.", users.Count);
            return users;
        }

        /// <inheritdoc />
        public async Task<ApplicationUser?> GetUserForEditAsync(string userId)
        {
            _logger.LogDebug("Retrieving user {UserId} for edit.", userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found.", userId);
                return null;
            }

            // Populate role information
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Any())
            {
                var role = await _roleManager.FindByNameAsync(userRoles.First());
                user.RoleId = role?.Id;
            }

            // Populate role list for dropdown
            user.RoleList = await GetRoleListAsync();
            return user;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateUserAsync(ApplicationUser updatedUser, string currentUserId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Updating user {UserId} by admin {AdminId}.", updatedUser.Id, currentUserId);

                // Fetch existing user
                var user = await _userManager.FindByIdAsync(updatedUser.Id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for update.", updatedUser.Id);
                    return false;
                }

                // Update name
                user.Name = updatedUser.Name;

                // Update role if changed
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (updatedUser.RoleId != null)
                {
                    var newRole = await _roleManager.FindByIdAsync(updatedUser.RoleId);
                    if (newRole != null)
                    {
                        if (currentRoles.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        }
                        await _userManager.AddToRoleAsync(user, newRole.Name);
                    }
                }

                // Save user update
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update user {UserId}: {Errors}", updatedUser.Id, string.Join(", ", result.Errors));
                    return false;
                }

                // Log activity
                await _adminActivityService.LogActivityAsync(
                    currentUserId,
                    "EditUser",
                    $"Edit User (Email: '{user.Email}')",
                    ipAddress);

                _logger.LogInformation("User {UserId} updated successfully.", updatedUser.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}.", updatedUser.Id);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteUserAsync(string userId, string currentUserId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Deleting user {UserId} by admin {AdminId}.", userId, currentUserId);

                // Fetch user
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for deletion.", userId);
                    return false;
                }

                // Delete associated user devices (if any)
                var userDevices = await _userDeviceRepository.GetAllAsync(d => d.UserId == userId);
                if (userDevices.Any())
                {
                    await _userDeviceRepository.DeleteRangeAsync(userDevices);
                }

                // Delete user
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to delete user {UserId}: {Errors}", userId, string.Join(", ", result.Errors));
                    return false;
                }

                // Log activity
                await _adminActivityService.LogActivityAsync(
                    currentUserId,
                    "DeleteUser",
                    $"Delete User (Email: '{user.Email}')",
                    ipAddress);

                _logger.LogInformation("User {UserId} deleted successfully.", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}.", userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<(bool Success, bool IsLocked, string UserName)> LockUnlockUserAsync(string userId, string currentUserId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Lock/unlock user {UserId} by admin {AdminId}.", userId, currentUserId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for lock/unlock.", userId);
                    return (false, false, null);
                }

                bool isCurrentlyLocked = user.LockoutEnd != null && user.LockoutEnd > DateTime.Now;
                bool newLockStatus;

                if (isCurrentlyLocked)
                {
                    // Unlock
                    user.LockoutEnd = DateTime.Now;
                    newLockStatus = false;
                    await _adminActivityService.LogActivityAsync(
                        currentUserId,
                        "UnlockUser",
                        $"Unlock User (Email: '{user.Email}')",
                        ipAddress);
                }
                else
                {
                    // Lock for 10 days
                    user.LockoutEnd = DateTime.Now.AddDays(10);
                    newLockStatus = true;
                    await _adminActivityService.LogActivityAsync(
                        currentUserId,
                        "LockUser",
                        $"Lock User (Email: '{user.Email}')",
                        ipAddress);
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update lock status for user {UserId}.", userId);
                    return (false, newLockStatus, user.Name);
                }

                _logger.LogInformation("User {UserId} lock status set to {IsLocked}.", userId, newLockStatus);
                return (true, newLockStatus, user.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling lock for user {UserId}.", userId);
                return (false, false, null);
            }
        }

        #endregion
    }
}