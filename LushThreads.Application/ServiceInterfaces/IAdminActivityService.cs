using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Defines the contract for admin activity-related business operations.
    /// </summary>
    public interface IAdminActivityService
    {
        #region Query Methods

        /// <summary>
        /// Retrieves all admin activities ordered by date descending, including user details.
        /// </summary>
        /// <returns>Collection of admin activities.</returns>
        Task<IEnumerable<AdminActivity>> GetAllActivitiesAsync();

        /// <summary>
        /// Retrieves activities for a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>Collection of activities for the user.</returns>
        Task<IEnumerable<AdminActivity>> GetActivitiesByUserIdAsync(string userId);

        /// <summary>
        /// Retrieves activities within a date range.
        /// </summary>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>Collection of activities within the date range.</returns>
        Task<IEnumerable<AdminActivity>> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Command Methods

        /// <summary>
        /// Logs an admin activity.
        /// </summary>
        /// <param name="userId">ID of the user performing the action.</param>
        /// <param name="activityType">Type of activity (e.g., AddProduct, UpdateBrand).</param>
        /// <param name="description">Description of the activity.</param>
        /// <param name="ipAddress">IP address from which the action was performed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogActivityAsync(string userId, string activityType, string description, string? ipAddress);

        /// <summary>
        /// Deletes old activities older than a specified date (for cleanup).
        /// </summary>
        /// <param name="cutoffDate">Date cutoff for deletion.</param>
        /// <returns>Number of deleted records.</returns>
        Task<int> DeleteOldActivitiesAsync(DateTime cutoffDate);

        #endregion
    }
}
