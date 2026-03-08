using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.UserAnalytics;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service responsible for providing user analytics data including user growth,
    /// roles distribution, account status, registration sources, and recent activity.
    /// </summary>
    public class UserAnalyticsService : IUserAnalyticsService
    {
        #region Fields

        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAnalyticsService"/> class.
        /// </summary>
        /// <param name="userRepository">Repository for user operations.</param>
        /// <param name="roleRepository">Repository for role operations.</param>
        /// <param name="userManager">User manager for Identity operations.</param>
        public UserAnalyticsService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userManager = userManager;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<UserAnalyticsViewModel> GetUserAnalytics(int days)
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var model = new UserAnalyticsViewModel
            {
                UserGrowth = await GetUserGrowthData(startDate, endDate, days),
                RolesDistribution = await GetRolesDistributionData(),
                AccountStatus = await GetAccountStatusData(),
                RegistrationSources = GetRegistrationSourcesData(),
                RecentActivity = await GetRecentActivityData(),
                TotalUsers = await _userRepository.GetTotalUsersCountAsync()
            };

            return model;
        }

        /// <inheritdoc />
        public async Task<UserGrowthViewModel> GetUserGrowthData(DateTime startDate, DateTime endDate, int days)
        {
            var interval = days <= 7 ? "day" : days <= 30 ? "week" : "month";

            var userGrowth = new UserGrowthViewModel
            {
                Labels = new List<string>(),
                NewUsers = new List<int>(),
                TotalUsers = new List<int>()
            };

            // Generate date labels
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                if (interval == "day")
                {
                    userGrowth.Labels.Add(currentDate.ToString("MMM d"));
                    currentDate = currentDate.AddDays(1);
                }
                else if (interval == "week")
                {
                    userGrowth.Labels.Add($"Week {GetWeekOfMonth(currentDate)}");
                    currentDate = currentDate.AddDays(7);
                }
                else
                {
                    userGrowth.Labels.Add(currentDate.ToString("MMM yyyy"));
                    currentDate = currentDate.AddMonths(1);
                }
            }

            // Get new users per interval
            currentDate = startDate;
            var totalUsers = 0;
            for (int i = 0; i < userGrowth.Labels.Count; i++)
            {
                DateTime nextDate = interval switch
                {
                    "day" => currentDate.AddDays(1),
                    "week" => currentDate.AddDays(7),
                    _ => currentDate.AddMonths(1)
                };

                var newUsersCount = await _userRepository.GetNewUsersCountAsync(currentDate, nextDate);

                totalUsers += newUsersCount;

                userGrowth.NewUsers.Add(newUsersCount);
                userGrowth.TotalUsers.Add(totalUsers);

                currentDate = nextDate;
            }

            return userGrowth;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates the week of the month for a given date.
        /// </summary>
        private int GetWeekOfMonth(DateTime date)
        {
            date = date.Date;
            DateTime firstMonthDay = new DateTime(date.Year, date.Month, 1);
            DateTime firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            if (firstMonthMonday > date)
            {
                firstMonthDay = firstMonthDay.AddMonths(-1);
                firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            }
            return (date - firstMonthMonday).Days / 7 + 1;
        }

        /// <summary>
        /// Retrieves the distribution of users across roles.
        /// </summary>
        private async Task<UserRolesDistributionViewModel> GetRolesDistributionData()
        {
            // 1. Get all existing roles from the database
            var existingRoles = await _roleRepository.GetAllRoleNamesAsync();

            // 2. Define role display names mapping
            var roleDisplayNames = new Dictionary<string, string>
            {
                {"Admin", "Administrator"},
                {"User", "Regular User"}
                // Add more mappings as needed
            };

            // 3. Handle case where there are many roles
            var roleCounts = new List<int>();
            var finalRoleNames = new List<string>();
            var colors = new List<string> { "#6366F1", "#10B981", "#3B82F6", "#F59E0B", "#EC4899", "#F97316" };

            const int otherRolesThreshold = 5; // Combine roles beyond this count into "Other"

            if (existingRoles.Count > otherRolesThreshold)
            {
                // Split into main roles and other roles
                var mainRoles = existingRoles.Take(otherRolesThreshold - 1).ToList();
                var otherRoles = existingRoles.Skip(otherRolesThreshold - 1).ToList();

                // Process main roles
                foreach (var role in mainRoles)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    roleCounts.Add(usersInRole.Count);

                    // Apply display name if available
                    finalRoleNames.Add(
                        roleDisplayNames.TryGetValue(role, out var displayName)
                            ? displayName
                            : role
                    );
                }

                // Combine remaining roles into "Other"
                var otherCount = 0;
                foreach (var role in otherRoles)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    otherCount += usersInRole.Count;
                }
                roleCounts.Add(otherCount);
                finalRoleNames.Add("Other");
            }
            else
            {
                // Process all roles normally
                foreach (var role in existingRoles)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    roleCounts.Add(usersInRole.Count);

                    // Apply display name if available
                    finalRoleNames.Add(
                        roleDisplayNames.TryGetValue(role, out var displayName)
                            ? displayName
                            : role
                    );
                }
            }

            // 4. Prepare colors (only use as many as we need)
            var roleColors = colors.Take(finalRoleNames.Count).ToList();

            return new UserRolesDistributionViewModel
            {
                RoleNames = finalRoleNames,
                Counts = roleCounts,
                Colors = roleColors
            };
        }

        /// <summary>
        /// Retrieves account status data (active vs locked users).
        /// </summary>
        private async Task<UserStatusViewModel> GetAccountStatusData()
        {
            var activeUsers = await _userRepository.GetActiveUsersCountAsync();
            var lockedUsers = await _userRepository.GetLockedUsersCountAsync();

            return new UserStatusViewModel
            {
                ActiveUsers = activeUsers,
                LockedUsers = lockedUsers
            };
        }

        /// <summary>
        /// Retrieves registration sources data.
        /// Note: In a real application, this should come from a database table tracking registration sources.
        /// </summary>
        private RegistrationSourcesViewModel GetRegistrationSourcesData()
        {
            // In a real app, you would query this from your database
            // This is a simplified version
            return new RegistrationSourcesViewModel
            {
                Sources = new List<string> { "Website", "Mobile App", "Admin Created", "API" },
                Counts = new List<int> { 120, 80, 35, 15 },
                Colors = new List<string> { "#3B82F6", "#8B5CF6", "#10B981", "#F59E0B" }
            };
        }

        /// <summary>
        /// Retrieves recent activity data (new users, logins, purchases).
        /// Note: Login and purchase data require additional tracking; these are placeholders.
        /// </summary>
        private async Task<RecentActivityViewModel> GetRecentActivityData()
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var last24Hours = DateTime.UtcNow.AddHours(-24);

            var newUsersToday = await _userRepository.GetNewUsersCountAsync(today, today.AddDays(1));

            // For logins, you would need to track login activity separately
            // This is a placeholder - you would need to implement actual login tracking
            var loginsLast24Hours = 124; // Placeholder

            // For purchases, you would query your order system
            var purchasesYesterday = 42; // Placeholder

            return new RecentActivityViewModel
            {
                NewUsersToday = newUsersToday,
                LoginsLast24Hours = loginsLast24Hours,
                PurchasesYesterday = purchasesYesterday
            };
        }

        #endregion
    }
}