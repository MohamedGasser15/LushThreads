using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.ViewModels.ProductAnalytics;
using LushThreads.Infrastructure.Persistence.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service responsible for providing product analytics data including key metrics,
    /// top products, inventory status, sales by category, and low stock items.
    /// </summary>
    public class ProductAnalyticsService : IProductAnalyticsService
    {
        #region Fields

        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private const int LowStockThreshold = 10;
        private const int OutOfStockThreshold = 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductAnalyticsService"/> class.
        /// </summary>
        /// <param name="productRepository">Repository for product operations.</param>
        /// <param name="stockRepository">Repository for stock operations.</param>
        /// <param name="orderDetailRepository">Repository for order detail operations.</param>
        public ProductAnalyticsService(
            IProductRepository productRepository,
            IStockRepository stockRepository,
            IOrderDetailRepository orderDetailRepository)
        {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _orderDetailRepository = orderDetailRepository;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves product analytics data for the specified number of days.
        /// </summary>
        /// <param name="days">Number of days to look back for analytics (default is 30).</param>
        /// <returns>A <see cref="ProductAnalyticsViewModel"/> containing all analytics data.</returns>
        public async Task<ProductAnalyticsViewModel> GetProductAnalytics(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);

            return new ProductAnalyticsViewModel
            {
                KeyMetrics = await GetKeyMetrics(cutoffDate),
                TopProducts = await GetTopProductsData(cutoffDate),
                InventoryStatus = await GetInventoryStatusData(),
                SalesByCategory = await GetSalesByCategoryData(cutoffDate),
                LowStockItems = await GetLowStockItems()
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates key metrics such as total products, top selling product, inventory value, and low stock count.
        /// </summary>
        /// <param name="cutoffDate">The date from which to consider orders.</param>
        /// <returns>A <see cref="KeyMetrics"/> object containing the metrics.</returns>
        private async Task<KeyMetrics> GetKeyMetrics(DateTime cutoffDate)
        {
            var totalProducts = await _productRepository.GetTotalProductsCountAsync();

            var topProduct = await _orderDetailRepository.GetTopProductAsync(cutoffDate);

            var inventoryValue = await _stockRepository.GetInventoryValueAsync();

            var lowStockCount = await _stockRepository.GetLowStockCountAsync(LowStockThreshold);

            return new KeyMetrics
            {
                TotalProducts = totalProducts,
                TopSellingProduct = topProduct?.ProductName ?? "N/A",
                TopProductSales = topProduct?.SalesCount ?? 0,
                InventoryValue = inventoryValue,
                InventoryChange = 0, // يمكن حساب التغيير لاحقًا
                LowStockItems = lowStockCount
            };
        }

        /// <summary>
        /// Retrieves the top selling products based on sales count.
        /// </summary>
        /// <param name="cutoffDate">The date from which to consider orders.</param>
        /// <param name="count">Number of top products to retrieve (default is 5).</param>
        /// <returns>A <see cref="TopProductsData"/> object containing top products data.</returns>
        private async Task<TopProductsData> GetTopProductsData(DateTime cutoffDate, int count = 5)
        {
            var topProducts = await _orderDetailRepository.GetTopProductsAsync(cutoffDate, count);

            var totalSales = topProducts.Sum(x => x.SalesCount);

            return new TopProductsData
            {
                ProductNames = topProducts.Select(x => x.ProductName).ToList(),
                SalesCounts = topProducts.Select(x => x.SalesCount).ToList(),
                RevenueGenerated = topProducts.Select(x => x.Revenue).ToList(),
                TotalSales = totalSales
            };
        }

        /// <summary>
        /// Retrieves inventory status data including counts for in stock, low stock, and out of stock items.
        /// </summary>
        /// <returns>An <see cref="InventoryStatusData"/> object containing inventory status.</returns>
        private async Task<InventoryStatusData> GetInventoryStatusData()
        {
            var inStock = await _stockRepository.GetInStockCountAsync(LowStockThreshold);
            var lowStock = await _stockRepository.GetLowStockCountAsync(LowStockThreshold, OutOfStockThreshold);
            var outOfStock = await _stockRepository.GetOutOfStockCountAsync(OutOfStockThreshold);

            return new InventoryStatusData
            {
                Statuses = new List<string> { "In Stock", "Low Stock", "Out of Stock" },
                Counts = new List<int> { inStock, lowStock, outOfStock },
                Colors = new List<string> { "#10B981", "#F59E0B", "#EF4444" },
                TotalItems = inStock + lowStock + outOfStock
            };
        }

        /// <summary>
        /// Retrieves sales data grouped by category.
        /// </summary>
        /// <param name="cutoffDate">The date from which to consider orders.</param>
        /// <returns>A <see cref="SalesByCategoryData"/> object containing sales by category.</returns>
        private async Task<SalesByCategoryData> GetSalesByCategoryData(DateTime cutoffDate)
        {
            var salesByCategory = await _orderDetailRepository.GetSalesByCategoryAsync(cutoffDate);

            var colors = new List<string> { "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6" };

            return new SalesByCategoryData
            {
                CategoryNames = salesByCategory.Select(x => x.CategoryName).ToList(),
                SalesCounts = salesByCategory.Select(x => x.SalesCount).ToList(),
                Revenue = salesByCategory.Select(x => x.Revenue).ToList(),
                Colors = colors.Take(salesByCategory.Count).ToList(),
                TotalSales = salesByCategory.Sum(x => x.SalesCount)
            };
        }

        /// <summary>
        /// Retrieves a list of items that are low in stock.
        /// </summary>
        /// <returns>A list of <see cref="LowStockItem"/> objects.</returns>
        private async Task<List<LowStockItem>> GetLowStockItems()
        {
            return await _stockRepository.GetLowStockItemsAsync(LowStockThreshold);
        }

        #endregion
    }
}