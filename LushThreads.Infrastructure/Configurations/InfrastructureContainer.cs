using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using LushThreads.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LushThreads.Infrastructure.Configurations
{
    /// <summary>
    /// Handles dependency injection for the Infrastructure layer.
    /// Registers DbContext, Identity, Stripe settings, and repositories.
    /// </summary>
    public static class InfrastructureContainer
    {
        /// <summary>
        /// Registers all Infrastructure-level services into the DI container.
        /// </summary>
        /// <param name="services">The service collection used for dependency injection.</param>
        /// <param name="configuration">The application configuration for accessing connection strings and settings.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register ApplicationDbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString")));

            // Register ASP.NET Core Identity with ApplicationUser and IdentityRole
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity options (password rules, user settings)
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            // Register Stripe settings from configuration (e.g., appsettings.json)
            services.Configure<StripeSettings>(configuration.GetSection("Stripe"));

            // Register generic repository and specific brand repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IAdminActivityRepository, AdminActivityRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<ISecurityActivityRepository, SecurityActivityRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            return services;
        }
    }
}