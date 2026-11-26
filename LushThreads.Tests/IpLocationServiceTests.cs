using LushThreads.Application.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace LushThreads.Tests
{
    public class IpLocationServiceTests
    {
        [Fact]
        public async Task GetLocationFromIpAsync_ReturnsLocalNetwork_ForLocalhost()
        {
            var factoryMock = new Mock<IHttpClientFactory>();
            var sut = new IpLocationService(factoryMock.Object);

            var result = await sut.GetLocationFromIpAsync("127.0.0.1");

            Assert.Contains("Local", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetLocationFromIpAsync_ParsesCityAndCountry_FromApiResponse()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"status\":\"success\",\"city\":\"Cairo\",\"country\":\"Egypt\"}", Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var sut = new IpLocationService(factoryMock.Object);
            var result = await sut.GetLocationFromIpAsync("41.41.41.41");

            Assert.Contains("Cairo", result);
            Assert.Contains("Egypt", result);
        }
    }
}
