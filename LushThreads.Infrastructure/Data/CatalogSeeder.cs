using LushThreads.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LushThreads.Infrastructure.Data
{
    /// <summary>
    /// Seeds brands, categories, products, and stocks from Data/SeedData/catalog.json.
    /// </summary>
    public static class CatalogSeeder
    {
        private sealed class CatalogSeedPayload
        {
            public List<BrandSeed> Brands { get; set; } = new();
            public List<CategorySeed> Categories { get; set; } = new();
            public List<ProductSeed> Products { get; set; } = new();
            public List<StockSeed> Stocks { get; set; } = new();
        }

        private sealed class BrandSeed
        {
            public int Brand_Id { get; set; }
            public string Brand_Name { get; set; } = string.Empty;
        }

        private sealed class CategorySeed
        {
            public int Category_Id { get; set; }
            public string Category_Name { get; set; } = string.Empty;
            public int? ParentCategoryId { get; set; }
        }

        private sealed class ProductSeed
        {
            public int Product_Id { get; set; }
            public string Product_Name { get; set; } = string.Empty;
            public string Product_Description { get; set; } = string.Empty;
            public decimal Product_Price { get; set; }
            public int Product_Rating { get; set; }
            public string imgUrl { get; set; } = string.Empty;
            public string Product_Color { get; set; } = string.Empty;
            public int Category_Id { get; set; }
            public int Brand_Id { get; set; }
            public bool IsFeatured { get; set; }
            public DateTime DateAdded { get; set; }
        }

        private sealed class StockSeed
        {
            public int Stock_Id { get; set; }
            public string Size { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public int Product_Id { get; set; }
        }

        /// <summary>
        /// Seeds catalog data if the Products table is currently empty.
        /// </summary>
        public static async Task SeedCatalogAsync(ApplicationDbContext context, ILogger logger)
        {
            if (await context.Products.AnyAsync())
            {
                logger.LogInformation("Products already exist. Skipping catalog seed.");
                return;
            }

            var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "SeedData", "catalog.json");
            if (!File.Exists(seedPath))
            {
                logger.LogWarning("Catalog seed file not found at {SeedPath}.", seedPath);
                return;
            }

            var json = await File.ReadAllTextAsync(seedPath);
            var payload = JsonSerializer.Deserialize<CatalogSeedPayload>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null)
                return;

            var brandIdMap = await SeedBrandsAsync(context, payload.Brands);
            var categoryIdMap = await SeedCategoriesAsync(context, payload.Categories);
            var productIdMap = await SeedProductsAsync(context, payload.Products, brandIdMap, categoryIdMap);
            await SeedStocksAsync(context, payload.Stocks, productIdMap);

            logger.LogInformation(
                "Catalog seeded: {Brands} brands, {Categories} categories, {Products} products, {Stocks} stocks.",
                brandIdMap.Count, categoryIdMap.Count, productIdMap.Count, payload.Stocks.Count);
        }

        private static async Task<Dictionary<int, int>> SeedBrandsAsync(ApplicationDbContext context, List<BrandSeed> seeds)
        {
            var existing = await context.Brands.ToDictionaryAsync(b => b.Brand_Name, StringComparer.OrdinalIgnoreCase);
            var map = new Dictionary<int, int>();
            var toInsert = new List<(int OldId, Brand Entity)>();

            foreach (var s in seeds)
            {
                if (existing.TryGetValue(s.Brand_Name, out var found))
                {
                    map[s.Brand_Id] = found.Brand_Id;
                }
                else
                {
                    var entity = new Brand { Brand_Name = s.Brand_Name };
                    existing[s.Brand_Name] = entity;
                    toInsert.Add((s.Brand_Id, entity));
                }
            }

            if (toInsert.Count > 0)
            {
                context.Brands.AddRange(toInsert.Select(x => x.Entity));
                await context.SaveChangesAsync();
                foreach (var item in toInsert)
                    map[item.OldId] = item.Entity.Brand_Id;
            }

            return map;
        }

        private static async Task<Dictionary<int, int>> SeedCategoriesAsync(ApplicationDbContext context, List<CategorySeed> seeds)
        {
            var existing = await context.Categories.ToListAsync();
            var map = new Dictionary<int, int>();

            // Pass 1: top-level categories
            var topSeeds = seeds.Where(c => c.ParentCategoryId == null).ToList();
            var topInsert = new List<(int OldId, Category Entity)>();
            foreach (var s in topSeeds)
            {
                var found = existing.FirstOrDefault(c =>
                    c.ParentCategoryId == null &&
                    string.Equals(c.Category_Name, s.Category_Name, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    map[s.Category_Id] = found.Category_Id;
                }
                else
                {
                    var entity = new Category { Category_Name = s.Category_Name, ParentCategoryId = null };
                    existing.Add(entity);
                    topInsert.Add((s.Category_Id, entity));
                }
            }

            if (topInsert.Count > 0)
            {
                context.Categories.AddRange(topInsert.Select(x => x.Entity));
                await context.SaveChangesAsync();
                foreach (var item in topInsert)
                    map[item.OldId] = item.Entity.Category_Id;
            }

            // Pass 2: child categories
            var childSeeds = seeds.Where(c => c.ParentCategoryId != null).ToList();
            var childInsert = new List<(int OldId, Category Entity)>();
            foreach (var s in childSeeds)
            {
                if (!map.TryGetValue(s.ParentCategoryId!.Value, out var newParentId))
                    continue;

                var found = existing.FirstOrDefault(c =>
                    c.ParentCategoryId == newParentId &&
                    string.Equals(c.Category_Name, s.Category_Name, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    map[s.Category_Id] = found.Category_Id;
                }
                else
                {
                    var entity = new Category { Category_Name = s.Category_Name, ParentCategoryId = newParentId };
                    existing.Add(entity);
                    childInsert.Add((s.Category_Id, entity));
                }
            }

            if (childInsert.Count > 0)
            {
                context.Categories.AddRange(childInsert.Select(x => x.Entity));
                await context.SaveChangesAsync();
                foreach (var item in childInsert)
                    map[item.OldId] = item.Entity.Category_Id;
            }

            return map;
        }

        private static async Task<Dictionary<int, int>> SeedProductsAsync(
            ApplicationDbContext context,
            List<ProductSeed> seeds,
            Dictionary<int, int> brandIdMap,
            Dictionary<int, int> categoryIdMap)
        {
            var map = new Dictionary<int, int>();
            var toInsert = new List<(int OldId, Product Entity)>();

            foreach (var s in seeds)
            {
                if (!brandIdMap.TryGetValue(s.Brand_Id, out var newBrandId) ||
                    !categoryIdMap.TryGetValue(s.Category_Id, out var newCategoryId))
                {
                    continue;
                }

                var entity = new Product
                {
                    Product_Name = s.Product_Name,
                    Product_Description = s.Product_Description,
                    Product_Price = s.Product_Price,
                    Product_Rating = s.Product_Rating,
                    imgUrl = s.imgUrl,
                    Product_Color = s.Product_Color,
                    brand_Id = newBrandId,
                    Category_Id = newCategoryId,
                    IsFeatured = s.IsFeatured,
                    DateAdded = s.DateAdded
                };
                toInsert.Add((s.Product_Id, entity));
            }

            if (toInsert.Count > 0)
            {
                context.Products.AddRange(toInsert.Select(x => x.Entity));
                await context.SaveChangesAsync();
                foreach (var item in toInsert)
                    map[item.OldId] = item.Entity.Product_Id;
            }

            return map;
        }

        private static async Task SeedStocksAsync(
            ApplicationDbContext context,
            List<StockSeed> seeds,
            Dictionary<int, int> productIdMap)
        {
            var stocks = new List<Stock>();
            foreach (var s in seeds)
            {
                if (!productIdMap.TryGetValue(s.Product_Id, out var newProductId))
                    continue;

                stocks.Add(new Stock
                {
                    Size = s.Size,
                    Quantity = s.Quantity,
                    Product_Id = newProductId
                });
            }

            if (stocks.Count > 0)
            {
                context.Stocks.AddRange(stocks);
                await context.SaveChangesAsync();
            }
        }
    }
}
