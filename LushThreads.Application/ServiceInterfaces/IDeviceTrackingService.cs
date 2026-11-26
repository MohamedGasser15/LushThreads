using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Service for tracking user devices and login activities.
    /// </summary>
    public interface IDeviceTrackingService
    {
        /// <summary>
        /// Tracks user device information and logs security activity.
        /// </summary>
        /// <param name="user">The application user.</param>
        /// <param name="httpContext">The HTTP context to extract request data.</param>
        /// <param name="deviceToken">Optional device token from cookie.</param>
        /// <returns>Returns the new or updated device token.</returns>
        Task<string> TrackUserDeviceAsync(ApplicationUser user, HttpContext httpContext, string? deviceToken);

        /// <summary>
        /// Gets the client IP address from the HTTP context.
        /// </summary>
        string GetClientIpAddress(HttpContext httpContext);
    }

}
