using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            // Add distributed memory cache and session services
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}