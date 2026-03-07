using Microsoft.Extensions.DependencyInjection;


namespace LushThreads.Application.Configurations
{
    /// <summary>
    /// Handles dependency injection for Application layer services.
    /// </summary>
    public static class ServiceContainer
    {
        /// <summary>
        /// Registers all Application-level services into the DI container.
        /// </summary>
        /// <param name="services">The service collection used for dependency injection.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMemoryCache();

            return services;
        }
    }
}
