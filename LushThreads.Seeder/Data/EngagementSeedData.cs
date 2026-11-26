using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LushThreads.Seeder.Data
{
    public static class EngagementSeedData
    {
        private static readonly string[] DeviceNames = { "iPhone 15", "Samsung Galaxy S24", "MacBook Pro", "Windows Desktop", "iPad Air", "Pixel 8" };
        private static readonly string[] DeviceTypes = { "Mobile", "Desktop", "Tablet" };
        private static readonly string[] OperatingSystems = { "iOS 17", "Android 14", "Windows 11", "macOS Sonoma", "iPadOS 17" };
        private static readonly string[] Browsers = { "Chrome 124", "Safari 17", "Edge 124", "Firefox 125" };
        private static readonly string[] Locations = { "Cairo, Egypt", "Alexandria, Egypt", "Riyadh, Saudi Arabia", "Dubai, UAE", "Amman, Jordan" };
        private static readonly string[] CardBrands = { "visa", "mastercard", "amex" };
        private static readonly string[] Sizes = { "S", "M", "L", "XL" };

        public static async Task SeedAsync(
            ApplicationDbContext context,
            int targetDevices,
            int targetPaymentMethods,
            int targetCartItems)
        {
            var users = await context.Set<ApplicationUser>()
                .Select(u => new
                {
                    u.Id,
                    Name = u.Name ?? "Customer",
                    Street = u.StreetAddress ?? "14 El-Tahrir St",
                    Country = u.Country ?? "Egypt",
                    Postal = u.PostalCode ?? "11511",
                    Phone = u.PhoneNumber ?? "+201000000000"
                })
                .Take(2000)
                .ToListAsync();

            if (users.Count == 0)
            {
                Console.WriteLine("[5/6] No users found. Skipping devices, payment methods, and cart items.");
                return;
            }

            var rng = new Random(2026);

            // 1. UserDevices
            var existingDevices = await context.UserDevices.CountAsync();
            if (existingDevices < targetDevices)
            {
                var toAdd = targetDevices - existingDevices;
                var devices = new List<UserDevice>(toAdd);
                for (int i = 0; i < toAdd; i++)
                {
                    var u = users[rng.Next(users.Count)];
                    var firstLogin = DateTime.UtcNow.AddDays(-rng.Next(1, 180));
                    devices.Add(new UserDevice
                    {
                        Id = Guid.NewGuid(),
                        UserId = u.Id,
                        DeviceName = DeviceNames[rng.Next(DeviceNames.Length)],
                        DeviceType = DeviceTypes[rng.Next(DeviceTypes.Length)],
                        OS = OperatingSystems[rng.Next(OperatingSystems.Length)],
                        Browser = Browsers[rng.Next(Browsers.Length)],
                        IpAddress = $"41.{rng.Next(32, 240)}.{rng.Next(0, 255)}.{rng.Next(1, 254)}",
                        Location = Locations[rng.Next(Locations.Length)],
                        FirstLoginDate = firstLogin,
                        LastLoginDate = firstLogin.AddDays(rng.Next(0, 30)),
                        DeviceToken = Guid.NewGuid().ToString("N")
                    });
                }

                context.UserDevices.AddRange(devices);
                await context.SaveChangesAsync();
                Console.WriteLine($"[5/6] Inserted {devices.Count} user devices (total: {existingDevices + devices.Count}).");
            }

            // 2. PaymentMethods
            var existingPayments = await context.PaymentMethods.CountAsync();
            if (existingPayments < targetPaymentMethods)
            {
                var toAdd = targetPaymentMethods - existingPayments;
                var methods = new List<PaymentMethod>(toAdd);
                for (int i = 0; i < toAdd; i++)
                {
                    var u = users[rng.Next(users.Count)];
                    methods.Add(new PaymentMethod
                    {
                        UserId = u.Id,
                        StripePaymentMethodId = $"pm_seed_{Guid.NewGuid():N}"[..24],
                        StripeCustomerId = $"cus_seed_{Guid.NewGuid():N}"[..22],
                        CardBrand = CardBrands[rng.Next(CardBrands.Length)],
                        Last4 = rng.Next(1000, 9999).ToString(),
                        ExpMonth = rng.Next(1, 13),
                        ExpYear = DateTime.UtcNow.Year + rng.Next(1, 5),
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(1, 180)),
                        BillingName = u.Name,
                        AddressLine1 = u.Street,
                        City = "Cairo",
                        Country = u.Country,
                        PostalCode = u.Postal,
                        PhoneNumber = u.Phone
                    });
                }

                context.PaymentMethods.AddRange(methods);
                await context.SaveChangesAsync();
                Console.WriteLine($"[5/6] Inserted {methods.Count} payment methods (total: {existingPayments + methods.Count}).");
            }

            // 3. CartItems
            var existingCarts = await context.CartItems.CountAsync();
            var productIds = await context.Products.Select(p => p.Product_Id).ToListAsync();
            if (existingCarts < targetCartItems && productIds.Count > 0)
            {
                var toAdd = targetCartItems - existingCarts;
                var items = new List<CartItem>(toAdd);
                for (int i = 0; i < toAdd; i++)
                {
                    var u = users[rng.Next(users.Count)];
                    items.Add(new CartItem
                    {
                        UserId = u.Id,
                        ProductId = productIds[rng.Next(productIds.Count)],
                        Quantity = rng.Next(1, 4),
                        Size = Sizes[rng.Next(Sizes.Length)]
                    });
                }

                context.CartItems.AddRange(items);
                await context.SaveChangesAsync();
                Console.WriteLine($"[5/6] Inserted {items.Count} cart items (total: {existingCarts + items.Count}).");
            }
        }
    }
}
