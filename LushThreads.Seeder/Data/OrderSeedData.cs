using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LushThreads.Seeder.Data
{
    public static class OrderSeedData
    {
        private static readonly string[] Carriers = { "Aramex", "DHL", "FedEx", "Bosta" };
        private static readonly string[] Sizes = { "S", "M", "L", "XL" };

        public static async Task SeedAsync(ApplicationDbContext context, int targetOrders)
        {
            var existingCount = await context.OrderHeaders.CountAsync();
            if (existingCount >= targetOrders)
            {
                Console.WriteLine($"[4/5] Orders already at {existingCount} (>= {targetOrders}). Skipping order seed.");
                return;
            }

            var users = await context.Set<ApplicationUser>()
                .Select(u => new
                {
                    u.Id,
                    Name = u.Name ?? "Customer",
                    Phone = u.PhoneNumber ?? "+201000000000",
                    Street = u.StreetAddress ?? "14 El-Tahrir St",
                    Country = u.Country ?? "Egypt",
                    Postal = u.PostalCode ?? "11511"
                })
                .Take(2000)
                .ToListAsync();

            var products = await context.Products
                .Select(p => new { p.Product_Id, p.Product_Price })
                .ToListAsync();

            if (users.Count == 0 || products.Count == 0)
            {
                Console.WriteLine("[4/5] Missing users or products. Skipping order seed.");
                return;
            }

            var toCreate = targetOrders - existingCount;
            var rng = new Random(77);
            var statusWeights = new[]
            {
                SD.StatusDelivered, SD.StatusDelivered, SD.StatusDelivered,
                SD.StatusShipped, SD.StatusShipped,
                SD.StatusInProcess,
                SD.StatusApproved,
                SD.StatusPending,
                SD.StatusCancelled,
                SD.StatusRefunded
            };

            const int batchSize = 300;
            var totalInserted = 0;

            for (int offset = 0; offset < toCreate; offset += batchSize)
            {
                var currentBatchCount = Math.Min(batchSize, toCreate - offset);
                var pendingHeaders = new List<(OrderHeader Header, List<OrderDetail> Details)>(currentBatchCount);

                for (int i = 0; i < currentBatchCount; i++)
                {
                    var user = users[rng.Next(users.Count)];
                    var orderDate = DateTime.UtcNow
                        .AddDays(-rng.Next(0, 365))
                        .Date
                        .AddHours(rng.Next(8, 23))
                        .AddMinutes(rng.Next(0, 60));

                    var status = statusWeights[rng.Next(statusWeights.Length)];
                    var paymentStatus = status == SD.StatusPending
                        ? SD.PaymentStatusPending
                        : (status == SD.StatusCancelled && rng.NextDouble() < 0.4
                            ? SD.PaymentStatusRejected
                            : SD.PaymentStatusApproved);

                    var itemCount = rng.Next(1, 5);
                    var details = new List<OrderDetail>(itemCount);
                    double subtotal = 0;

                    for (int j = 0; j < itemCount; j++)
                    {
                        var prod = products[rng.Next(products.Count)];
                        var qty = rng.Next(1, 4);
                        var unitPrice = (double)prod.Product_Price;
                        subtotal += unitPrice * qty;

                        details.Add(new OrderDetail
                        {
                            ProductId = prod.Product_Id,
                            Count = qty,
                            Size = Sizes[rng.Next(Sizes.Length)],
                            price = unitPrice
                        });
                    }

                    subtotal = Math.Round(subtotal, 2);
                    var shippingFee = subtotal >= 150 ? 0.0 : 12.0;
                    var tax = Math.Round(subtotal * 0.08, 2);
                    var total = Math.Round(subtotal + shippingFee + tax, 2);

                    var isShipped = status == SD.StatusShipped || status == SD.StatusDelivered;

                    var header = new OrderHeader
                    {
                        ApplicationUserId = user.Id,
                        OrderDate = orderDate,
                        ShippingDate = isShipped ? orderDate.AddDays(rng.Next(1, 4)) : orderDate.AddDays(3),
                        PaymentDate = paymentStatus == SD.PaymentStatusApproved ? orderDate : default,
                        PaymentDueDate = DateOnly.FromDateTime(orderDate.AddDays(7)),
                        Subtotal = subtotal,
                        ShippingFee = shippingFee,
                        Tax = tax,
                        OrderTotal = total,
                        OrderStatus = status,
                        PaymentStatus = paymentStatus,
                        Carrier = isShipped ? Carriers[rng.Next(Carriers.Length)] : null,
                        TrackingNumber = isShipped ? $"LT{rng.Next(10000000, 99999999)}" : null,
                        PhoneNumber = user.Phone,
                        StreetAddress = user.Street,
                        Country = user.Country,
                        PostalCode = user.Postal,
                        Name = user.Name
                    };

                    pendingHeaders.Add((header, details));
                }

                context.OrderHeaders.AddRange(pendingHeaders.Select(x => x.Header));
                await context.SaveChangesAsync();

                var allDetails = new List<OrderDetail>();
                foreach (var (header, details) in pendingHeaders)
                {
                    foreach (var d in details)
                    {
                        d.OrderHeaderId = header.Id;
                        allDetails.Add(d);
                    }
                }

                context.OrderDetails.AddRange(allDetails);
                await context.SaveChangesAsync();
                totalInserted += currentBatchCount;
            }

            Console.WriteLine($"[4/5] Inserted {totalInserted} orders with details (total orders: {existingCount + totalInserted}).");
        }
    }
}
