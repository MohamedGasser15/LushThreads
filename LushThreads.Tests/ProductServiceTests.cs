using LushThreads.Application.Interfaces;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace LushThreads.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IBrandRepository> _brandRepoMock = new();
        private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
        private readonly Mock<IStockRepository> _stockRepoMock = new();
        private readonly Mock<IAdminActivityService> _activityMock = new();
        private readonly Mock<IWebHostEnvironment> _envMock = new();
        private readonly Mock<ILogger<ProductService>> _loggerMock = new();
        private readonly ProductService _sut;

        public ProductServiceTests()
        {
            _sut = new ProductService(
                _productRepoMock.Object,
                _brandRepoMock.Object,
                _categoryRepoMock.Object,
                _stockRepoMock.Object,
                _activityMock.Object,
                _envMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ToggleFeaturedAsync_FlipsIsFeaturedAndUpdates()
        {
            var product = new Product { Product_Id = 3, Product_Name = "Jacket", IsFeatured = false };
            _productRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), null, false, default))
                .ReturnsAsync(product);

            var newStatus = await _sut.ToggleFeaturedAsync(3, "admin-1", "127.0.0.1");

            Assert.True(newStatus);
            Assert.True(product.IsFeatured);
            _productRepoMock.Verify(r => r.UpdateAsync(product, default), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsNull_WhenMissing()
        {
            _productRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), null, false, default))
                .ReturnsAsync((Product)null!);

            var result = await _sut.GetProductByIdAsync(404);

            Assert.Null(result);
        }
    }
}
