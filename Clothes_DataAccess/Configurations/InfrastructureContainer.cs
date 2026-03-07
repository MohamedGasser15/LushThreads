
using LushThreads.Infrastructure.Persistence.IRepository;
using LushThreads.Infrastructure.Persistence.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace LushThreads.Infrastructure.Configurations
{
    /// <summary>
    /// Handles dependency injection for Infrastructure Repositories.
    /// </summary>
    public static class InfrastructureContainer
    {
        /// <summary>
        /// Registers all repository dependencies into the DI container.
        /// </summary>
        /// <param name="services">The service collection used for dependency injection.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            return services;
        }
    }
}
