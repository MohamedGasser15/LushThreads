using LushThreads.Infrastructure.Data;
using LushThreads.Seeder.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace LushThreads.Seeder.Services
{
    public sealed class SeedOptions
    {
        public int TargetUsers { get; init; } = 2000;
        public int TargetProducts { get; init; } = 300;
        public int TargetOrders { get; init; } = 1500;
        public int TargetDevices { get; init; } = 1600;
        public int TargetPaymentMethods { get; init; } = 700;
        public int TargetCartItems { get; init; } = 800;

        public static SeedOptions FromUserCount(int users, int? products = null, int? orders = null)
        {
            return new SeedOptions
            {
                TargetUsers = users,
                TargetProducts = products ?? Math.Max(160, (int)(users * 0.15)),
                TargetOrders = orders ?? Math.Max(300, (int)(users * 0.75)),
                TargetDevices = Math.Max(200, (int)(users * 0.80)),
                TargetPaymentMethods = Math.Max(100, (int)(users * 0.35)),
                TargetCartItems = Math.Max(100, (int)(users * 0.40))
            };
        }
    }

    public static class SeedEngine
    {
        public static async Task RunAsync(IServiceProvider serviceProvider, SeedOptions options)
        {
            var sw = Stopwatch.StartNew();
            Console.WriteLine("=== LushThreads Full Database Seeder ===");
            Console.WriteLine(
                $"Targets -> Users: {options.TargetUsers}, Products: {options.TargetProducts}, Orders: {options.TargetOrders}, " +
                $"Devices: {options.TargetDevices}, PaymentMethods: {options.TargetPaymentMethods}, CartItems: {options.TargetCartItems}");
            Console.WriteLine();

            await RoleAndAdminSeedData.SeedAsync(serviceProvider);

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await ProductBulkSeedData.SeedAsync(context, options.TargetProducts);
            await UserBulkSeedData.SeedAsync(context, options.TargetUsers);
            await OrderSeedData.SeedAsync(context, options.TargetOrders);
            await EngagementSeedData.SeedAsync(
                context,
                options.TargetDevices,
                options.TargetPaymentMethods,
                options.TargetCartItems);
            await ActivitySeedData.SeedAsync(context);

            sw.Stop();
            Console.WriteLine();
            Console.WriteLine($"All 12 tables seeded successfully in {sw.Elapsed.TotalSeconds:F2}s.");
        }
    }
}
