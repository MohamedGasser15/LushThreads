using LushThreads.Application.Interfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace LushThreads.Tests
{
    public class HomeServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
        private readonly Mock<IBrandRepository> _brandRepoMock = new();
        private readonly Mock<ILogger<HomeService>> _loggerMock = new();
        private readonly HomeService _sut;

        public HomeServiceTests()
        {
            _sut = new HomeService(
                _productRepoMock.Object,
                _categoryRepoMock.Object,
                _brandRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetHomeDataAsync_ReturnsFeaturedAndNewArrivals()
        {
            var sampleProducts = new List<Product>
            {
                new()
                {
                    Product_Id = 1,
                    Product_Name = "Classic Tee",
                    Product_Price = 29.99m,
                    Product_Rating = 5,
                    imgUrl = "/img/p1.jpg",
                    IsFeatured = true,
                    Brand = new Brand { Brand_Name = "Nike" },
                    Category = new Category { Category_Name = "T-Shirts" },
                    Stocks = new List<Stock> { new() { Size = "M", Quantity = 10 } }
                }
            };

            _productRepoMock
                .Setup(r => r.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string?>(),
                    false,
                    It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                    null,
                    8,
                    default))
                .ReturnsAsync(sampleProducts);

            var result = await _sut.GetHomeViewModelAsync(null);

            Assert.Single(result.FeaturedProducts);
            Assert.Single(result.NewArrivals);
            Assert.Equal("Classic Tee", result.FeaturedProducts[0].Product_Name);
            Assert.Equal("Nike", result.FeaturedProducts[0].BrandName);
        }

        [Fact]
        public async Task GetShopProductsAsync_ReturnsPagedResultsAndTotalPages()
        {
            var allProducts = Enumerable.Range(1, 25).Select(i => new Product
            {
                Product_Id = i,
                Product_Name = $"Product {i}",
                Product_Price = 49.99m,
                imgUrl = "/img/p.jpg",
                Brand = new Brand { Brand_Name = "Nike" },
                Category = new Category { Category_Name = "Tops" },
                Stocks = new List<Stock> { new() { Size = "L", Quantity = 5 } }
            }).ToList();

            _productRepoMock
                .Setup(r => r.GetAllAsync(
                    null,
                    null,
                    false,
                    null,
                    null,
                    null,
                    default))
                .ReturnsAsync(allProducts);

            _productRepoMock
                .Setup(r => r.GetAllAsync(
                    null,
                    "Brand,Category,Stocks",
                    false,
                    It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                    0,
                    18,
                    default))
                .ReturnsAsync(allProducts.Take(18).ToList());

            var (products, totalPages, totalProducts) = await _sut.GetShopProductsAsync(null, page: 1, pageSize: 18);

            Assert.Equal(18, products.Count());
            Assert.Equal(2, totalPages);
            Assert.Equal(25, totalProducts);
        }
    }
}
