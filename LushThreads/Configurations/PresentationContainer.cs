using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.RateLimiting;

namespace LushThreads.Web.Configurations
{
    /// <summary>
    /// Handles dependency injection for the Presentation (Web) layer.
    /// Registers MVC, external authentication (Facebook, Google), session, and HTTP context accessor.
    /// </summary>
    public static class PresentationContainer
    {
        /// <summary>
        /// Registers all Presentation-level services into the DI container.
        /// </summary>
        /// <param name="services">The service collection used for dependency injection.</param>
        /// <param name="configuration">The application configuration for accessing external authentication keys.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add MVC controllers with views
            services.AddControllersWithViews();

            // Configure external authentication providers
            services.AddAuthentication()
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.ClientId = configuration.GetSection("Facebook:ClientId").Value;
                    facebookOptions.ClientSecret = configuration.GetSection("Facebook:ClientSecret").Value;
                })
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = configuration.GetSection("Google:ClientId").Value;
                    googleOptions.ClientSecret = configuration.GetSection("Google:ClientSecret").Value;
                });

            // Add distributed memory cache and hardened session services
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            // Add response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // Add rate limiting for authentication endpoints
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("auth", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });

            services.AddHttpContextAccessor();

            return services;
        }
    }
}