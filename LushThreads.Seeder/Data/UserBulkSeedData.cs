using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LushThreads.Seeder.Data
{
    public static class UserBulkSeedData
    {
        private static readonly string[] FirstNames =
        {
            "Ahmed", "Mohamed", "Omar", "Youssef", "Karim", "Hassan", "Tarek", "Nour",
            "Salma", "Mariam", "Sara", "Laila", "Khaled", "Mostafa", "Heba", "Nadine",
            "Amr", "Ziad", "Rana", "Farida"
        };

        private static readonly string[] LastNames =
        {
            "Gasser", "Ali", "Ibrahim", "Mahmoud", "Mostafa", "Adel", "Nabil",
            "Fathy", "Samir", "Khaled", "Mansour", "Farouk", "Soliman", "El-Sayed"
        };

        private static readonly string[] Countries =
        {
            "Egypt", "Saudi Arabia", "UAE", "Kuwait", "Jordan", "Qatar"
        };

        private static readonly string[] Streets =
        {
            "14 El-Tahrir St", "22 Nile Corniche", "8 Nasr City Rd",
            "45 Maadi Degla St", "19 Zamalek 26th July", "31 Heliopolis Merghany"
        };

        public static async Task SeedAsync(ApplicationDbContext context, int targetCount)
        {
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == SD.User);
            if (userRole == null)
            {
                Console.WriteLine("[3/5] User role not found. Skipping bulk users.");
                return;
            }

            var existingCount = await context.Users.CountAsync();
            if (existingCount >= targetCount)
            {
                Console.WriteLine($"[3/5] Users already at {existingCount} (>= {targetCount}). Skipping bulk users.");
                return;
            }

            var existingEmails = new HashSet<string>(
                await context.Users.Select(u => u.NormalizedEmail ?? string.Empty).ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            var hasher = new PasswordHasher<ApplicationUser>();
            var sharedPasswordHash = hasher.HashPassword(new ApplicationUser(), "SeedUser123!");
            var rng = new Random(42);
            var toCreate = targetCount - existingCount;
            var usersToAdd = new List<ApplicationUser>(toCreate);

            int index = 1;
            while (usersToAdd.Count < toCreate)
            {
                var email = $"seeduser{index}@lushthreads.local";
                index++;
                var normalized = email.ToUpperInvariant();
                if (!existingEmails.Add(normalized))
                    continue;

                var first = FirstNames[rng.Next(FirstNames.Length)];
                var last = LastNames[rng.Next(LastNames.Length)];

                usersToAdd.Add(new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    NormalizedUserName = normalized,
                    Email = email,
                    NormalizedEmail = normalized,
                    EmailConfirmed = true,
                    PasswordHash = sharedPasswordHash,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Name = $"{first} {last}",
                    PhoneNumber = $"+2010{rng.Next(10000000, 99999999)}",
                    StreetAddress = Streets[rng.Next(Streets.Length)],
                    Country = Countries[rng.Next(Countries.Length)],
                    PostalCode = rng.Next(11000, 99999).ToString(),
                    CreatedDate = DateTime.UtcNow.AddDays(-rng.Next(0, 365)).AddHours(-rng.Next(0, 24))
                });
            }

            const int batchSize = 500;
            for (int i = 0; i < usersToAdd.Count; i += batchSize)
            {
                var batch = usersToAdd.Skip(i).Take(batchSize).ToList();
                context.Users.AddRange(batch);
                await context.SaveChangesAsync();

                context.UserRoles.AddRange(batch.Select(u => new IdentityUserRole<string>
                {
                    UserId = u.Id,
                    RoleId = userRole.Id
                }));
                await context.SaveChangesAsync();
            }

            Console.WriteLine($"[3/5] Inserted {usersToAdd.Count} bulk users (total users: {existingCount + usersToAdd.Count}).");
        }
    }
}
