using LushThreads.Application.Interfaces;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace LushThreads.Tests
{
    public class BrandServiceTests
    {
        private readonly Mock<IBrandRepository> _brandRepoMock = new();
        private readonly Mock<IAdminActivityService> _activityMock = new();
        private readonly Mock<ILogger<BrandService>> _loggerMock = new();
        private readonly BrandService _sut;

        public BrandServiceTests()
        {
            _sut = new BrandService(_brandRepoMock.Object, _activityMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllBrandsAsync_ReturnsAllBrands()
        {
            var expected = new List<Brand>
            {
                new() { Brand_Id = 1, Brand_Name = "Nike" },
                new() { Brand_Id = 2, Brand_Name = "Adidas" }
            };

            _brandRepoMock
                .Setup(r => r.GetAllAsync(null, null, false, null, null, null, default))
                .ReturnsAsync(expected);

            var result = (await _sut.GetAllBrandsAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Nike", result[0].Brand_Name);
        }

        [Fact]
        public async Task GetBrandByIdAsync_ReturnsNull_WhenNotFound()
        {
            _brandRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Brand, bool>>>(), null, false, default))
                .ReturnsAsync((Brand)null!);

            var result = await _sut.GetBrandByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateBrandAsync_CreatesBrandAndLogsActivity()
        {
            var brand = new Brand { Brand_Id = 5, Brand_Name = "Zara" };

            await _sut.CreateBrandAsync(brand, "admin-1", "127.0.0.1");

            _brandRepoMock.Verify(r => r.CreateAsync(brand, default), Times.Once);
            _activityMock.Verify(
                a => a.LogActivityAsync("admin-1", "AddBrand", It.Is<string>(s => s.Contains("5")), "127.0.0.1"),
                Times.Once);
        }
    }
}
