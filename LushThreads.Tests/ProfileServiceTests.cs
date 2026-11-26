using LushThreads.Application.Interfaces;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LushThreads.Tests
{
    public class ProfileServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock = TestHelpers.CreateUserManagerMock();
        private readonly Mock<IOrderRepository> _orderRepoMock = new();
        private readonly Mock<IOrderDetailRepository> _detailRepoMock = new();
        private readonly Mock<IUserDeviceRepository> _deviceRepoMock = new();
        private readonly Mock<ISecurityActivityRepository> _secRepoMock = new();
        private readonly Mock<IAdminActivityService> _activityMock = new();
        private readonly Mock<IEmailSender> _emailMock = new();
        private readonly Mock<IEmailTemplateService> _templateMock = new();
        private readonly Mock<ILogger<ProfileService>> _loggerMock = new();
        private readonly ProfileService _sut;

        public ProfileServiceTests()
        {
            _sut = new ProfileService(
                _userManagerMock.Object,
                _orderRepoMock.Object,
                _detailRepoMock.Object,
                _deviceRepoMock.Object,
                _secRepoMock.Object,
                _activityMock.Object,
                _emailMock.Object,
                _templateMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public void IsValidEmail_ValidatesCorrectly()
        {
            Assert.True(_sut.IsValidEmail("user@lushthreads.com"));
            Assert.False(_sut.IsValidEmail("invalid-email"));
        }

        [Fact]
        public async Task VerifyEmailCodeAsync_ReturnsTrue_WhenCodeMatches()
        {
            var result = await _sut.VerifyEmailCodeAsync("u-1", "123456", "123456");
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyAndEnable2FAAsync_ReturnsFalse_WhenExpired()
        {
            var result = await _sut.VerifyAndEnable2FAAsync("u-1", "123456", "123456", DateTime.UtcNow.AddMinutes(-5));
            Assert.False(result);
        }
    }
}
