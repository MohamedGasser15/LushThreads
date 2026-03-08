using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service class responsible for business logic related to AdminActivity entities.
    /// Implements <see cref="IAdminActivityService"/>.
    /// </summary>
    public class AdminActivityService : IAdminActivityService
    {
        #region Fields

        private readonly IAdminActivityRepository _adminActivityRepository;
        private readonly ILogger<AdminActivityService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminActivityService"/> class.
        /// </summary>
        /// <param name="adminActivityRepository">Repository for AdminActivity operations.</param>
        /// <param name="logger">Logger instance.</param>
        public AdminActivityService(
            IAdminActivityRepository adminActivityRepository,
            ILogger<AdminActivityService> logger)
        {
            _adminActivityRepository = adminActivityRepository;
            _logger = logger;
        }

        #endregion

        #region Query Methods

        /// <inheritdoc />
        public async Task<IEnumerable<AdminActivity>> GetAllActivitiesAsync()
        {
            _logger.LogDebug("Retrieving all admin activities with user details.");
            var activities = await _adminActivityRepository.GetAllAsync(
                includeProperties: "User",
                orderBy: q => q.OrderByDescending(a => a.ActivityDate)
            );
            _logger.LogDebug("Retrieved {Count} admin activities.", activities.Count);
            return activities;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AdminActivity>> GetActivitiesByUserIdAsync(string userId)
        {
            _logger.LogDebug("Retrieving admin activities for user ID {UserId}.", userId);
            var activities = await _adminActivityRepository.GetAllAsync(
                filter: a => a.UserId == userId,
                includeProperties: "User",
                orderBy: q => q.OrderByDescending(a => a.ActivityDate)
            );
            _logger.LogDebug("Retrieved {Count} activities for user {UserId}.", activities.Count, userId);
            return activities;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AdminActivity>> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug("Retrieving admin activities from {StartDate} to {EndDate}.", startDate, endDate);
            var activities = await _adminActivityRepository.GetAllAsync(
                filter: a => a.ActivityDate >= startDate && a.ActivityDate <= endDate,
                includeProperties: "User",
                orderBy: q => q.OrderByDescending(a => a.ActivityDate)
            );
            _logger.LogDebug("Retrieved {Count} activities in date range.", activities.Count);
            return activities;
        }

        #endregion

        #region Command Methods

        /// <inheritdoc />
        public async Task LogActivityAsync(string userId, string activityType, string description, string? ipAddress)
        {
            try
            {
                _logger.LogDebug("Logging admin activity: {ActivityType} for user {UserId}.", activityType, userId);

                var activity = new AdminActivity
                {
                    UserId = userId,
                    ActivityType = activityType,
                    Description = description,
                    IpAddress = ipAddress ?? "Unknown",
                    ActivityDate = DateTime.Now
                };

                await _adminActivityRepository.CreateAsync(activity);

                _logger.LogInformation("Admin activity logged successfully: {ActivityType} for user {UserId}.", activityType, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging admin activity: {ActivityType} for user {UserId}.", activityType, userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> DeleteOldActivitiesAsync(DateTime cutoffDate)
        {
            try
            {
                _logger.LogInformation("Deleting admin activities older than {CutoffDate}.", cutoffDate);

                var oldActivities = await _adminActivityRepository.GetAllAsync(
                    filter: a => a.ActivityDate < cutoffDate,
                    isTracking: true // Need tracking for deletion
                );

                if (oldActivities.Any())
                {
                    await _adminActivityRepository.DeleteRangeAsync(oldActivities);
                    _logger.LogInformation("Deleted {Count} old admin activities.", oldActivities.Count);
                    return oldActivities.Count;
                }

                _logger.LogDebug("No old activities found for deletion.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting old admin activities.");
                throw;
            }
        }

        #endregion
    }
}