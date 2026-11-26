using LushThreads.Application.Interfaces;
using LushThreads.Application.Services;
using LushThreads.Application.ViewModels.ProductAnalytics;
using LushThreads.Domain.Entites;
using Moq;

namespace LushThreads.Tests
{
    public class ProductAnalyticsServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IStockRepository> _stockRepoMock = new();
        private readonly Mock<IOrderDetailRepository> _detailRepoMock = new();
        private readonly ProductAnalyticsService _sut;

        public ProductAnalyticsServiceTests()
        {
            _sut = new ProductAnalyticsService(_productRepoMock.Object, _stockRepoMock.Object, _detailRepoMock.Object);
        }

        [Fact]
        public async Task GetProductAnalytics_ReturnsKeyMetricsAndLowStockItems()
        {
            _productRepoMock
                .Setup(r => r.GetAllAsync(null, null, false, null, null, null, default))
                .ReturnsAsync(new List<Product>
                {
                    new() { Product_Id = 1, Product_Name = "Tee", Product_Price = 20m, IsFeatured = true },
                    new() { Product_Id = 2, Product_Name = "Jeans", Product_Price = 50m, IsFeatured = false }
                });

            _productRepoMock
                .Setup(r => r.GetTotalProductsCountAsync())
                .ReturnsAsync(2);

            _stockRepoMock
                .Setup(r => r.GetLowStockCountAsync(10))
                .ReturnsAsync(1);

            _stockRepoMock
                .Setup(r => r.GetInventoryValueAsync())
                .ReturnsAsync(450m);

            _stockRepoMock
                .Setup(r => r.GetAllAsync(null, null, false, null, null, null, default))
                .ReturnsAsync(new List<Stock>
                {
                    new() { Product_Id = 1, Quantity = 0 },
                    new() { Product_Id = 2, Quantity = 5 }
                });

            _stockRepoMock
                .Setup(r => r.GetLowStockItemsAsync(10))
                .ReturnsAsync(new List<LowStockItem> { new() { ProductName = "Jeans", Size = "M", CurrentStock = 5, Threshold = 10 } });

            _detailRepoMock
                .Setup(r => r.GetTopProductAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(("Tee", 15, 300m));

            _detailRepoMock
                .Setup(r => r.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync(new List<TopProductResult>());

            _detailRepoMock
                .Setup(r => r.GetSalesByCategoryAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<CategorySalesResult>());

            var result = await _sut.GetProductAnalytics(30);

            Assert.Equal(2, result.KeyMetrics.TotalProducts);
            Assert.Equal(1, result.KeyMetrics.LowStockItems);
            Assert.Equal("Tee", result.KeyMetrics.TopSellingProduct);
            Assert.Single(result.LowStockItems);
        }
    }
}
