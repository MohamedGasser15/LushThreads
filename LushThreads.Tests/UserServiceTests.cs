using LushThreads.Application.Interfaces;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace LushThreads.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock = TestHelpers.CreateUserManagerMock();
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock = TestHelpers.CreateRoleManagerMock();
        private readonly Mock<IRoleRepository> _roleRepoMock = new();
        private readonly Mock<IUserDeviceRepository> _deviceRepoMock = new();
        private readonly Mock<IAdminActivityService> _activityMock = new();
        private readonly Mock<ILogger<UserService>> _loggerMock = new();
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _sut = new UserService(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _roleRepoMock.Object,
                _deviceRepoMock.Object,
                _activityMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_PopulatesRolesFromSingleBatchQuery()
        {
            var users = new List<ApplicationUser>
            {
                new() { Id = "u-1", Name = "Admin" },
                new() { Id = "u-2", Name = "Customer" }
            }.AsQueryable();

            _userManagerMock.Setup(m => m.Users).Returns(users);
            _roleRepoMock.Setup(r => r.GetUserRolesMapAsync()).ReturnsAsync(new Dictionary<string, string>
            {
                ["u-1"] = "Admin"
            });

            var result = (await _sut.GetAllUsersAsync()).ToList();

            Assert.Equal("Admin", result[0].Role);
            Assert.Equal("None", result[1].Role);
            _roleRepoMock.Verify(r => r.GetUserRolesMapAsync(), Times.Once);
        }

        [Fact]
        public async Task LockUnlockUserAsync_LocksUnlockedUser()
        {
            var user = new ApplicationUser { Id = "u-2", Name = "Ali", LockoutEnd = null };
            _userManagerMock.Setup(m => m.FindByIdAsync("u-2")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var (success, isLocked, userName) = await _sut.LockUnlockUserAsync("u-2", "admin-1", "127.0.0.1");

            Assert.True(success);
            Assert.True(isLocked);
            Assert.Equal("Ali", userName);
        }
    }
}
