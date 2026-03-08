using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LushThreads.Application.Configurations
{
    /// <summary>
    /// Handles dependency injection for the Application layer.
    /// Registers application services, memory cache, and email sender.
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
            // Add in-memory cache
            services.AddMemoryCache();

            // Register application services (business logic)
            services.AddScoped<IBrandService, BrandService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IUserAnalyticsService, UserAnalyticsService>();
            services.AddScoped<IOrderAnalyticsService, OrderAnalyticsService>();
            services.AddScoped<IProductAnalyticsService, ProductAnalyticsService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IAdminActivityService, AdminActivityService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IHomeService, HomeService>();
            return services;
        }
    }
}