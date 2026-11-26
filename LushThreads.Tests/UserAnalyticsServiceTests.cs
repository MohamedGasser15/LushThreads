using LushThreads.Application.Interfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace LushThreads.Tests
{
    public class UserAnalyticsServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IRoleRepository> _roleRepoMock = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock = TestHelpers.CreateUserManagerMock();
        private readonly UserAnalyticsService _sut;

        public UserAnalyticsServiceTests()
        {
            _sut = new UserAnalyticsService(_userRepoMock.Object, _roleRepoMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task GetUserGrowthData_ReturnsAlignedLabelsAndCounts()
        {
            var start = new DateTime(2026, 11, 1);
            var end = new DateTime(2026, 11, 3);

            _userRepoMock
                .Setup(r => r.GetTotalUsersCountAsync())
                .ReturnsAsync(100);

            _userRepoMock
                .Setup(r => r.GetUsersByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ApplicationUser>
                {
                    new() { Id = "u-1", CreatedDate = new DateTime(2026, 11, 1) },
                    new() { Id = "u-2", CreatedDate = new DateTime(2026, 11, 3) }
                });

            var result = await _sut.GetUserGrowthData(start, end, 3);

            Assert.NotEmpty(result.Labels);
            Assert.NotEmpty(result.NewUsers);
        }
    }
}
