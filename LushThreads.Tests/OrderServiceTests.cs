using LushThreads.Application.Interfaces;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace LushThreads.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock = new();
        private readonly Mock<IOrderDetailRepository> _detailRepoMock = new();
        private readonly Mock<IAdminActivityService> _activityMock = new();
        private readonly Mock<IEmailSender> _emailMock = new();
        private readonly Mock<IEmailTemplateService> _templateMock = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock = TestHelpers.CreateUserManagerMock();
        private readonly Mock<ILogger<OrderService>> _loggerMock = new();
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _sut = new OrderService(
                _orderRepoMock.Object,
                _detailRepoMock.Object,
                _activityMock.Object,
                _emailMock.Object,
                _templateMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ReturnsOrders()
        {
            _orderRepoMock
                .Setup(r => r.GetAllOrdersWithUserAsync())
                .ReturnsAsync(new List<OrderHeader> { new() { Id = 1 } });

            var orders = (await _sut.GetAllOrdersAsync()).ToList();

            Assert.Single(orders);
        }

        [Fact]
        public async Task ApproveOrderAsync_SetsStatusApprovedAndLogsActivity()
        {
            var user = new ApplicationUser { Id = "u-1", Email = "u@test.com", Name = "User" };
            var order = new OrderHeader { Id = 10, ApplicationUserId = "u-1", ApplicationUser = user, OrderStatus = SD.StatusPending };

            _orderRepoMock
                .Setup(r => r.GetOrderWithUserAsync(10))
                .ReturnsAsync(order);
            _orderRepoMock
                .Setup(r => r.GetOrderDetailsWithProductAsync(10))
                .ReturnsAsync(new List<OrderDetail>());
            _templateMock.Setup(t => t.GetOrderInProcessEmail(user, 10, It.IsAny<string>())).Returns("<html/>");

            await _sut.ApproveOrderAsync(10, "admin-1", "127.0.0.1");

            Assert.Equal(SD.StatusApproved, order.OrderStatus);
            Assert.Equal(SD.PaymentStatusApproved, order.PaymentStatus);
            _orderRepoMock.Verify(r => r.UpdateAsync(order, default), Times.Once);
            _activityMock.Verify(a => a.LogActivityAsync("admin-1", "OrderApproval", It.IsAny<string>(), "127.0.0.1"), Times.Once);
        }

        [Fact]
        public async Task ShipOrderAsync_Throws_WhenOrderNotApproved()
        {
            var order = new OrderHeader { Id = 11, OrderStatus = SD.StatusPending };
            _orderRepoMock.Setup(r => r.GetOrderWithUserAsync(11)).ReturnsAsync(order);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ShipOrderAsync(11, "admin-1", "127.0.0.1"));
        }

        [Fact]
        public async Task DeliverOrderAsync_SetsDelivered_WhenShipped()
        {
            var user = new ApplicationUser { Id = "u-1", Email = "u@test.com", Name = "User" };
            var order = new OrderHeader { Id = 12, ApplicationUser = user, OrderStatus = SD.StatusShipped };
            _orderRepoMock.Setup(r => r.GetOrderWithUserAsync(12)).ReturnsAsync(order);
            _templateMock.Setup(t => t.GetOrderDeliveredEmail(user, 12, It.IsAny<string>())).Returns("<html/>");

            await _sut.DeliverOrderAsync(12, "admin-1", "127.0.0.1");

            Assert.Equal(SD.StatusDelivered, order.OrderStatus);
            _orderRepoMock.Verify(r => r.UpdateAsync(order, default), Times.Once);
        }
    }
}
