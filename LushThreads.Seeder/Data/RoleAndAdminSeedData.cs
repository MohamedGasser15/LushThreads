using LushThreads.Infrastructure.Data;

namespace LushThreads.Seeder.Data
{
    public static class RoleAndAdminSeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            Console.WriteLine("[1/5] Seeding roles, admin user, and base catalog...");
            await DbInitializer.InitializeAsync(serviceProvider);
        }
    }
}
