using LushThreads.Application.Interfaces;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using LushThreads.Application.ViewModels.Auth;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LushThreads.Tests
{
    public class AccountServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock = TestHelpers.CreateUserManagerMock();
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock = TestHelpers.CreateRoleManagerMock();
        private readonly Mock<IEmailSender> _emailMock = new();
        private readonly Mock<IEmailTemplateService> _templateMock = new();
        private readonly Mock<ISecurityActivityRepository> _secRepoMock = new();
        private readonly Mock<IUserDeviceRepository> _deviceRepoMock = new();
        private readonly Mock<ILogger<AccountService>> _loggerMock = new();
        private readonly AccountService _sut;

        public AccountServiceTests()
        {
            _signInManagerMock = TestHelpers.CreateSignInManagerMock(_userManagerMock.Object);
            _sut = new AccountService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _roleManagerMock.Object,
                _emailMock.Object,
                _templateMock.Object,
                _secRepoMock.Object,
                _deviceRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ValidateLoginAsync_Fails_WhenUserNotFound()
        {
            _userManagerMock.Setup(m => m.FindByEmailAsync("nobody@test.com")).ReturnsAsync((ApplicationUser?)null);

            var (success, errorMessage, _, _) = await _sut.ValidateLoginAsync("nobody@test.com", "Pass1234");

            Assert.False(success);
            Assert.NotNull(errorMessage);
        }

        [Fact]
        public async Task RegisterUserAsync_AllowsDuplicateName_AndUsesEmailAsUserName()
        {
            var model = new RegisterViewModel
            {
                Name = "Mohamed Gasser",
                Email = "new@test.com",
                Password = "Password1"
            };

            _userManagerMock.Setup(m => m.FindByEmailAsync("new@test.com")).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "Password1")).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.GetClaimsAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<System.Security.Claims.Claim>());
            _userManagerMock.Setup(m => m.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<System.Security.Claims.Claim>())).ReturnsAsync(IdentityResult.Success);

            var (success, _, user, code) = await _sut.RegisterUserAsync(model);

            Assert.True(success);
            Assert.Equal("new@test.com", user.UserName);
            Assert.Equal("Mohamed Gasser", user.Name);
            Assert.False(string.IsNullOrWhiteSpace(code));
        }

        [Fact]
        public async Task HandleExternalLoginCallbackAsync_ReturnsError_WhenRemoteErrorPresent()
        {
            var (success, error, _, _, _) = await _sut.HandleExternalLoginCallbackAsync("/", "access_denied");

            Assert.False(success);
            Assert.Contains("access_denied", error);
        }
    }
}
