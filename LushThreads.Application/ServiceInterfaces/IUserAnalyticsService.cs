using LushThreads.Domain.ViewModels.UserAnalytics;
using System;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Provides methods for retrieving user analytics data.
    /// </summary>
    public interface IUserAnalyticsService
    {
        /// <summary>
        /// Retrieves user analytics data for the specified number of days.
        /// </summary>
        /// <param name="days">Number of days to look back for analytics.</param>
        /// <returns>A <see cref="UserAnalyticsViewModel"/> containing all user analytics data.</returns>
        Task<UserAnalyticsViewModel> GetUserAnalytics(int days);

        /// <summary>
        /// Retrieves user growth data over a specified date range.
        /// </summary>
        /// <param name="startDate">Start date for the data.</param>
        /// <param name="endDate">End date for the data.</param>
        /// <param name="days">Total number of days (used to determine interval).</param>
        /// <returns>A <see cref="UserGrowthViewModel"/> containing growth data.</returns>
        Task<UserGrowthViewModel> GetUserGrowthData(DateTime startDate, DateTime endDate, int days);
    }
}