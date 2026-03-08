using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Service interface for user management operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all users with their roles.
        /// </summary>
        /// <returns>List of ApplicationUser objects with Role property populated.</returns>
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves a user by ID with role information for editing.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <returns>User object with RoleList and RoleId populated.</returns>
        Task<ApplicationUser?> GetUserForEditAsync(string userId);

        /// <summary>
        /// Updates a user's details and role.
        /// </summary>
        /// <param name="updatedUser">User object with updated values (Name, RoleId).</param>
        /// <param name="currentUserId">ID of the admin performing the update.</param>
        /// <param name="ipAddress">IP address of the admin.</param>
        /// <returns>True if successful.</returns>
        Task<bool> UpdateUserAsync(ApplicationUser updatedUser, string currentUserId, string ipAddress);

        /// <summary>
        /// Deletes a user and their associated devices.
        /// </summary>
        /// <param name="userId">ID of the user to delete.</param>
        /// <param name="currentUserId">ID of the admin performing the deletion.</param>
        /// <param name="ipAddress">IP address of the admin.</param>
        /// <returns>True if successful.</returns>
        Task<bool> DeleteUserAsync(string userId, string currentUserId, string ipAddress);

        /// <summary>
        /// Locks or unlocks a user account.
        /// </summary>
        /// <param name="userId">ID of the user to lock/unlock.</param>
        /// <param name="currentUserId">ID of the admin performing the action.</param>
        /// <param name="ipAddress">IP address of the admin.</param>
        /// <returns>Tuple containing success flag, new lock status, and user name.</returns>
        Task<(bool Success, bool IsLocked, string UserName)> LockUnlockUserAsync(string userId, string currentUserId, string ipAddress);
    }
}
