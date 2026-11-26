using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LushThreads.Seeder.Data
{
    public static class ActivitySeedData
    {
        private static readonly (string Type, string Description)[] AdminActions =
        {
            ("CreateProduct", "Added new seasonal collection product"),
            ("UpdateProduct", "Updated product pricing and featured status"),
            ("UpdateStock", "Restocked inventory sizes S-XL"),
            ("UpdateOrder", "Updated order shipping status to Shipped"),
            ("CreateBrand", "Added new partner fashion brand"),
            ("CreateCategory", "Created new apparel subcategory")
        };

        private static readonly (string Type, string Description)[] SecurityActions =
        {
            ("Login", "Successful account sign in"),
            ("PasswordChanged", "User updated account password"),
            ("TwoFactorEnabled", "Two-factor authentication enabled"),
            ("DeviceTrusted", "New browser device verified")
        };

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@example.com");
            if (admin != null && !await context.AdminActivities.AnyAsync())
            {
                var rng = new Random(99);
                var adminLogs = new List<AdminActivity>(50);
                for (int i = 0; i < 50; i++)
                {
                    var action = AdminActions[rng.Next(AdminActions.Length)];
                    adminLogs.Add(new AdminActivity
                    {
                        UserId = admin.Id,
                        ActivityType = action.Type,
                        Description = action.Description,
                        ActivityDate = DateTime.UtcNow.AddDays(-rng.Next(0, 60)).AddHours(-rng.Next(0, 24)),
                        IpAddress = $"192.168.1.{rng.Next(2, 250)}"
                    });
                }

                context.AdminActivities.AddRange(adminLogs);
                await context.SaveChangesAsync();
                Console.WriteLine($"[5/5] Inserted {adminLogs.Count} admin activity logs.");
            }
            else
            {
                Console.WriteLine("[5/5] Admin activity logs already exist or admin user missing.");
            }

            if (!await context.SecurityActivities.AnyAsync())
            {
                var sampleUsers = await context.Users.Select(u => u.Id).Take(100).ToListAsync();
                if (sampleUsers.Count > 0)
                {
                    var rng = new Random(123);
                    var secLogs = new List<SecurityActivity>(150);
                    for (int i = 0; i < 150; i++)
                    {
                        var action = SecurityActions[rng.Next(SecurityActions.Length)];
                        secLogs.Add(new SecurityActivity
                        {
                            UserId = sampleUsers[rng.Next(sampleUsers.Count)],
                            ActivityType = action.Type,
                            Description = action.Description,
                            ActivityDate = DateTime.UtcNow.AddDays(-rng.Next(0, 90)),
                            IpAddress = $"10.0.{rng.Next(0, 10)}.{rng.Next(2, 250)}"
                        });
                    }

                    context.SecurityActivities.AddRange(secLogs);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"[5/5] Inserted {secLogs.Count} security activity logs.");
                }
            }
        }
    }
}
