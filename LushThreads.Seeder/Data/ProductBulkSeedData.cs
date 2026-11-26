using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LushThreads.Seeder.Data
{
    public static class ProductBulkSeedData
    {
        private static readonly string[] Adjectives =
        {
            "Classic", "Essential", "Urban", "Premium", "Relaxed", "Tailored",
            "Vintage", "Modern", "Minimal", "Athletic", "Oversized", "Slim-Fit"
        };

        private static readonly string[] Garments =
        {
            "Cotton Tee", "Polo Shirt", "Denim Jacket", "Chino Pants", "Hoodie",
            "Crewneck Sweatshirt", "Linen Shirt", "Cargo Trousers", "Bomber Jacket",
            "Track Pants", "Zip Cardigan", "Oxford Shirt"
        };

        private static readonly string[] Colors =
        {
            "Black", "White", "Navy", "Olive", "Beige", "Charcoal", "Burgundy", "Sage"
        };

        private static readonly string[] Sizes = { "S", "M", "L", "XL", "XXL" };

        public static async Task SeedAsync(ApplicationDbContext context, int targetTotalProducts)
        {
            var currentCount = await context.Products.CountAsync();
            if (currentCount >= targetTotalProducts)
            {
                Console.WriteLine($"[2/5] Products already at {currentCount} (>= {targetTotalProducts}). Skipping extra products.");
                return;
            }

            var toCreate = targetTotalProducts - currentCount;
            var brandIds = await context.Brands.Select(b => b.Brand_Id).ToListAsync();
            var categoryIds = await context.Categories.Select(c => c.Category_Id).ToListAsync();
            var existingImages = await context.Products
                .Where(p => !string.IsNullOrEmpty(p.imgUrl))
                .Select(p => p.imgUrl)
                .Distinct()
                .ToListAsync();

            if (brandIds.Count == 0 || categoryIds.Count == 0 || existingImages.Count == 0)
            {
                Console.WriteLine("[2/5] Missing base brands/categories/images. Skipping extra products.");
                return;
            }

            var rng = new Random(101);
            var newProducts = new List<Product>(toCreate);

            for (int i = 1; i <= toCreate; i++)
            {
                var adj = Adjectives[rng.Next(Adjectives.Length)];
                var garment = Garments[rng.Next(Garments.Length)];
                var color = Colors[rng.Next(Colors.Length)];
                var price = Math.Round((decimal)(rng.Next(25, 180) + 0.99), 2);

                newProducts.Add(new Product
                {
                    Product_Name = $"{adj} {color} {garment} #{currentCount + i}",
                    Product_Description = $"Crafted from breathable everyday fabric, the {adj.ToLower()} {garment.ToLower()} delivers clean tailoring and lasting comfort.",
                    Product_Price = price,
                    Product_Rating = rng.Next(3, 6),
                    imgUrl = existingImages[rng.Next(existingImages.Count)],
                    Product_Color = color,
                    brand_Id = brandIds[rng.Next(brandIds.Count)],
                    Category_Id = categoryIds[rng.Next(categoryIds.Count)],
                    IsFeatured = rng.NextDouble() < 0.25,
                    DateAdded = DateTime.UtcNow.AddDays(-rng.Next(1, 180))
                });
            }

            context.Products.AddRange(newProducts);
            await context.SaveChangesAsync();

            var stocks = new List<Stock>(newProducts.Count * 4);
            foreach (var product in newProducts)
            {
                var sizeCount = rng.Next(3, Sizes.Length + 1);
                foreach (var size in Sizes.Take(sizeCount))
                {
                    stocks.Add(new Stock
                    {
                        Product_Id = product.Product_Id,
                        Size = size,
                        Quantity = rng.Next(0, 60)
                    });
                }
            }

            context.Stocks.AddRange(stocks);
            await context.SaveChangesAsync();

            Console.WriteLine($"[2/5] Added {newProducts.Count} extra products and {stocks.Count} stock entries (total products: {currentCount + newProducts.Count}).");
        }
    }
}
