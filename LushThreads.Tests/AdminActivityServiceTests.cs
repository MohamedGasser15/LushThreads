using LushThreads.Application.Interfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace LushThreads.Tests
{
    public class AdminActivityServiceTests
    {
        private readonly Mock<IAdminActivityRepository> _repoMock = new();
        private readonly Mock<ILogger<AdminActivityService>> _loggerMock = new();
        private readonly AdminActivityService _sut;

        public AdminActivityServiceTests()
        {
            _sut = new AdminActivityService(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task LogActivityAsync_CreatesActivity()
        {
            await _sut.LogActivityAsync("admin-1", "CreateProduct", "Created Hoodie", "127.0.0.1");

            _repoMock.Verify(r => r.CreateAsync(
                It.Is<AdminActivity>(a =>
                    a.UserId == "admin-1" &&
                    a.ActivityType == "CreateProduct" &&
                    a.Description == "Created Hoodie" &&
                    a.IpAddress == "127.0.0.1"),
                default), Times.Once);
        }

        [Fact]
        public async Task DeleteOldActivitiesAsync_DeletesMatchingActivitiesAndReturnsCount()
        {
            var old = new List<AdminActivity>
            {
                new() { UserId = "u1", ActivityType = "A", Description = "D", IpAddress = "1.1.1.1" },
                new() { UserId = "u1", ActivityType = "B", Description = "E", IpAddress = "1.1.1.1" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<AdminActivity, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>(), null, null, null, default))
                .ReturnsAsync(old);

            var deleted = await _sut.DeleteOldActivitiesAsync(DateTime.UtcNow.AddDays(-30));

            Assert.Equal(2, deleted);
            _repoMock.Verify(r => r.DeleteRangeAsync(old, default), Times.Once);
        }
    }
}
