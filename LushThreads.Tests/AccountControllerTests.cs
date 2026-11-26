using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.ViewModels.Auth;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Web.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LushThreads.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _accountServiceMock = new();
        private readonly Mock<IEmailTemplateService> _templateMock = new();
        private readonly Mock<IDeviceTrackingService> _deviceMock = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock = TestHelpers.CreateUserManagerMock();
        private readonly Mock<IEmailSender> _emailMock = new();
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly AccountController _sut;

        public AccountControllerTests()
        {
            _signInManagerMock = TestHelpers.CreateSignInManagerMock(_userManagerMock.Object);
            _sut = new AccountController(
                _accountServiceMock.Object,
                _templateMock.Object,
                _deviceMock.Object,
                _userManagerMock.Object,
                _emailMock.Object,
                _signInManagerMock.Object,
                NullLogger<AccountController>.Instance);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _sut.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Content("~/")).Returns("/");
            urlHelperMock.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/Admin/Analytics");
            _sut.Url = urlHelperMock.Object;
        }

        [Fact]
        public async Task Login_RedirectsAdminToAdminDashboard()
        {
            var admin = new ApplicationUser { Id = "a-1", Email = "admin@example.com", PreferredLanguage = "en" };
            _accountServiceMock
                .Setup(s => s.ValidateLoginAsync("admin@example.com", "Admin1234@"))
                .ReturnsAsync((true, null!, admin, false));
            _userManagerMock.Setup(m => m.IsInRoleAsync(admin, SD.Admin)).ReturnsAsync(true);

            var result = await _sut.Login(new LoginViewModel { Email = "admin@example.com", Password = "Admin1234@" });

            var redirect = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal("/Admin/Analytics", redirect.Url);
        }

        [Fact]
        public async Task Login_RedirectsRegularUserToHome()
        {
            var user = new ApplicationUser { Id = "u-1", Email = "user@test.com", PreferredLanguage = "en" };
            _accountServiceMock
                .Setup(s => s.ValidateLoginAsync("user@test.com", "User1234"))
                .ReturnsAsync((true, null!, user, false));
            _userManagerMock.Setup(m => m.IsInRoleAsync(user, SD.Admin)).ReturnsAsync(false);

            var result = await _sut.Login(new LoginViewModel { Email = "user@test.com", Password = "User1234" });

            var redirect = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal("/", redirect.Url);
        }

        [Fact]
        public async Task ExternalLoginCallback_ReturnsPopupHtml_WhenIsPopupTrue()
        {
            var user = new ApplicationUser { Id = "u-2", Email = "g@test.com" };
            _accountServiceMock
                .Setup(s => s.HandleExternalLoginCallbackAsync("/", null!))
                .ReturnsAsync((true, null!, user, "Google", "/"));
            _userManagerMock.Setup(m => m.IsInRoleAsync(user, SD.Admin)).ReturnsAsync(false);

            var result = await _sut.ExternalLoginCallback("/", null, isPopup: true);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/html", content.ContentType);
            Assert.Contains("external-login-success", content.Content);
        }

        private sealed class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();
            public bool IsAvailable => true;
            public string Id => "test-session";
            public IEnumerable<string> Keys => _store.Keys;
            public void Clear() => _store.Clear();
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => _store.Remove(key);
            public void Set(string key, byte[] value) => _store[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
        }
    }
}
