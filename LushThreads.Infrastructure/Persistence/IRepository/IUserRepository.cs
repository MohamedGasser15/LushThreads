using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    /// <summary>
    /// Repository interface for ApplicationUser entity.
    /// </summary>
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        /// <summary>
        /// Gets the total number of users.
        /// </summary>
        Task<int> GetTotalUsersCountAsync();

        /// <summary>
        /// Gets the count of new users registered between the specified dates.
        /// </summary>
        /// <param name="startDate">Start date (inclusive).</param>
        /// <param name="endDate">End date (exclusive).</param>
        Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the count of active users (not locked out).
        /// </summary>
        Task<int> GetActiveUsersCountAsync();

        /// <summary>
        /// Gets the count of locked out users.
        /// </summary>
        Task<int> GetLockedUsersCountAsync();

        /// <summary>
        /// Gets a list of users created within a date range.
        /// </summary>
        Task<List<ApplicationUser>> GetUsersByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
