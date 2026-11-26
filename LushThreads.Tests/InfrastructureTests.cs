using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LushThreads.Tests
{
    public class InfrastructureTests
    {
        private static ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task BrandRepository_CrudOperationsWork()
        {
            await using var db = CreateInMemoryContext();
            var repo = new BrandRepository(db, NullLogger<BrandRepository>.Instance);

            var brand = new Brand { Brand_Name = "Lush" };
            await repo.CreateAsync(brand);
            db.ChangeTracker.Clear();

            var fetched = await repo.GetAsync(b => b.Brand_Name == "Lush");
            Assert.NotNull(fetched);

            fetched!.Brand_Name = "Lush Updated";
            await repo.UpdateAsync(fetched);

            var all = await repo.GetAllAsync();
            Assert.Single(all);
            Assert.Equal("Lush Updated", all[0].Brand_Name);

            await repo.DeleteAsync(fetched);
            Assert.Empty(await repo.GetAllAsync());
        }

        [Fact]
        public async Task RoleRepository_GetUserRolesMapAsync_ReturnsExpectedRoles()
        {
            await using var db = CreateInMemoryContext();
            db.Roles.Add(new IdentityRole { Id = "r-1", Name = "Admin", NormalizedName = "ADMIN" });
            db.UserRoles.Add(new IdentityUserRole<string> { UserId = "u-1", RoleId = "r-1" });
            await db.SaveChangesAsync();

            var repo = new RoleRepository(db, NullLogger<RoleRepository>.Instance);
            var map = await repo.GetUserRolesMapAsync();

            Assert.True(map.TryGetValue("u-1", out var role));
            Assert.Equal("Admin", role);
        }

        [Fact]
        public async Task CatalogSeeder_SeedsCatalogFromJson_WhenEmpty()
        {
            await using var db = CreateInMemoryContext();

            await CatalogSeeder.SeedCatalogAsync(db, NullLogger.Instance);

            Assert.True(await db.Brands.CountAsync() > 0);
            Assert.True(await db.Categories.CountAsync() > 0);
            Assert.True(await db.Products.CountAsync() > 0);
            Assert.True(await db.Stocks.CountAsync() > 0);
        }
    }
}
