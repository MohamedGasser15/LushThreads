using LushThreads.Application.Interfaces;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Net;

namespace LushThreads.Tests
{
    public class DeviceTrackingServiceTests
    {
        private readonly Mock<IUserDeviceRepository> _deviceRepoMock = new();
        private readonly Mock<ISecurityActivityRepository> _securityRepoMock = new();
        private readonly Mock<IIpLocationService> _ipLocationMock = new();
        private readonly DeviceTrackingService _sut;

        public DeviceTrackingServiceTests()
        {
            _sut = new DeviceTrackingService(_deviceRepoMock.Object, _securityRepoMock.Object, _ipLocationMock.Object);
        }

        [Fact]
        public void GetClientIpAddress_PrefersXForwardedForHeader()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "41.10.20.30, 10.0.0.1";
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            var ip = _sut.GetClientIpAddress(context);

            Assert.Equal("41.10.20.30", ip);
        }

        [Fact]
        public async Task TrackUserDeviceAsync_CreatesNewDevice_WhenTokenNotFound()
        {
            var user = new ApplicationUser { Id = "u-1" };
            var context = new DefaultHttpContext();
            context.Request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/124.0";
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            _ipLocationMock.Setup(x => x.GetLocationFromIpAsync(It.IsAny<string>())).ReturnsAsync("Cairo, Egypt");
            _deviceRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<UserDevice, bool>>>(), null, false, default))
                .ReturnsAsync((UserDevice)null!);

            var token = await _sut.TrackUserDeviceAsync(user, context, null);

            Assert.False(string.IsNullOrWhiteSpace(token));
            _deviceRepoMock.Verify(r => r.CreateAsync(It.Is<UserDevice>(d => d.UserId == "u-1"), default), Times.Once);
            _securityRepoMock.Verify(r => r.CreateAsync(It.Is<SecurityActivity>(s => s.UserId == "u-1"), default), Times.Once);
        }
    }
}
