using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LushThreads.Infrastructure.Data
{
    /// <summary>
    /// Handles database seeding for roles, admin user, brands, and categories.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Initializes the database with required roles, admin user, and sample data.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve required services.</param>
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var context = services.GetRequiredService<ApplicationDbContext>();
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DbInitializer");

                // Seed roles and admin user
                await SeedRolesAsync(roleManager, logger);
                await SeedAdminUserAsync(userManager, roleManager, logger);

                // Seed sample brands and categories
                await SeedBrandsAsync(context, logger);
                await SeedCategoriesAsync(context, logger);
            }
            catch (Exception ex)
            {
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DbInitializer");
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        /// <summary>
        /// Seeds predefined roles (Admin, User) if they don't exist.
        /// </summary>
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            string[] roleNames = { SD.Admin, SD.User };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                }
                else
                {
                    logger.LogInformation("Role '{RoleName}' already exists.", roleName);
                }
            }
        }

        /// <summary>
        /// Seeds the default admin user if it doesn't exist and assigns the Admin role.
        /// </summary>
        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            const string adminEmail = "admin@example.com";
            const string adminPassword = "Admin1234@";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Name = "Admin",
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, SD.Admin);
                    logger.LogInformation("Admin user created successfully.");
                }
                else
                {
                    logger.LogError("Error creating admin user:");
                    foreach (var error in result.Errors)
                    {
                        logger.LogError("- {ErrorDescription}", error.Description);
                    }
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists.");
            }
        }

        /// <summary>
        /// Seeds sample brands if no brands exist.
        /// </summary>
        private static async Task SeedBrandsAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!await context.Brands.AnyAsync())
            {
                var brands = new List<Brand>
                {
                    new Brand { Brand_Name = "Nike" },
                    new Brand { Brand_Name = "Adidas" },
                    new Brand { Brand_Name = "Puma" },
                    new Brand { Brand_Name = "Under Armour" },
                    new Brand { Brand_Name = "Reebok" },
                    new Brand { Brand_Name = "New Balance" }
                };

                await context.Brands.AddRangeAsync(brands);
                await context.SaveChangesAsync();
                logger.LogInformation("Sample brands seeded successfully.");
            }
            else
            {
                logger.LogInformation("Brands already exist. Skipping seed.");
            }
        }

        /// <summary>
        /// Seeds sample categories (including parent-child relationships) if no categories exist.
        /// </summary>
        private static async Task SeedCategoriesAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!await context.Categories.AnyAsync())
            {
                // Create parent categories first
                var clothing = new Category { Category_Name = "Clothing" };
                var shoes = new Category { Category_Name = "Shoes" };
                var accessories = new Category { Category_Name = "Accessories" };

                await context.Categories.AddRangeAsync(clothing, shoes, accessories);
                await context.SaveChangesAsync(); // Save to generate IDs

                // Create subcategories
                var subCategories = new List<Category>
                {
                    new Category { Category_Name = "T-Shirts", ParentCategoryId = clothing.Category_Id },
                    new Category { Category_Name = "Jeans", ParentCategoryId = clothing.Category_Id },
                    new Category { Category_Name = "Jackets", ParentCategoryId = clothing.Category_Id },
                    new Category { Category_Name = "Running Shoes", ParentCategoryId = shoes.Category_Id },
                    new Category { Category_Name = "Casual Shoes", ParentCategoryId = shoes.Category_Id },
                    new Category { Category_Name = "Boots", ParentCategoryId = shoes.Category_Id },
                    new Category { Category_Name = "Bags", ParentCategoryId = accessories.Category_Id },
                    new Category { Category_Name = "Hats", ParentCategoryId = accessories.Category_Id },
                    new Category { Category_Name = "Socks", ParentCategoryId = accessories.Category_Id }
                };

                await context.Categories.AddRangeAsync(subCategories);
                await context.SaveChangesAsync();
                logger.LogInformation("Sample categories seeded successfully.");
            }
            else
            {
                logger.LogInformation("Categories already exist. Skipping seed.");
            }
        }
    }
}