using LushThreads.Application.Interfaces;
using LushThreads.Application.Services;
using LushThreads.Application.ViewModels.OrderAnalytics;
using Microsoft.Extensions.Logging;
using Moq;

namespace LushThreads.Tests
{
    public class OrderAnalyticsServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock = new();
        private readonly Mock<IOrderDetailRepository> _detailRepoMock = new();
        private readonly Mock<ILogger<OrderAnalyticsService>> _loggerMock = new();
        private readonly OrderAnalyticsService _sut;

        public OrderAnalyticsServiceTests()
        {
            _sut = new OrderAnalyticsService(_orderRepoMock.Object, _detailRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetOrderStatusData_MapsCountsAndStatuses()
        {
            _orderRepoMock
                .Setup(r => r.GetOrderStatusDistributionAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<OrderAnalyticsFilter>()))
                .ReturnsAsync(new List<OrderStatusGroup>
                {
                    new() { Status = "Delivered", Count = 10, Revenue = 500.0 },
                    new() { Status = "Pending", Count = 2, Revenue = 80.0 }
                });

            var result = await _sut.GetOrderStatusData(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

            Assert.NotEmpty(result.Statuses);
            var deliveredIdx = result.Statuses.IndexOf("Delivered");
            Assert.True(deliveredIdx >= 0);
            Assert.Equal(10, result.Counts[deliveredIdx]);
        }

        [Fact]
        public async Task GetRevenueTrendData_ReturnsLabelsAndRevenue()
        {
            _orderRepoMock
                .Setup(r => r.GetRevenueTrendAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<OrderAnalyticsFilter>(), It.IsAny<string>()))
                .ReturnsAsync(new List<RevenueTrendItem>
                {
                    new() { Label = "2026-11-25", Revenue = 120.0, OrderCount = 3 }
                });

            var result = await _sut.GetRevenueTrendData(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, 7);

            Assert.Single(result.Labels);
            Assert.Equal(120m, result.Revenue[0]);
        }
    }
}
