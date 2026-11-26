using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace LushThreads.Tests
{
    public class SettingServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock = TestHelpers.CreateUserManagerMock();
        private readonly Mock<ILogger<SettingService>> _loggerMock = new();
        private readonly SettingService _sut;

        public SettingServiceTests()
        {
            _sut = new SettingService(_userManagerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ChangeLanguageAsync_UpdatesUserLanguage()
        {
            var user = new ApplicationUser { Id = "u-1", PreferredLanguage = "en" };
            _userManagerMock.Setup(m => m.FindByIdAsync("u-1")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _sut.ChangeLanguageAsync("u-1", "ar");

            Assert.True(result.Succeeded);
            Assert.Equal("ar", user.PreferredLanguage);
        }

        [Fact]
        public async Task ChangeCurrencyAsync_ReturnsFailed_WhenUserNotFound()
        {
            _userManagerMock.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

            var result = await _sut.ChangeCurrencyAsync("missing", "EGP");

            Assert.False(result.Succeeded);
        }
    }
}
