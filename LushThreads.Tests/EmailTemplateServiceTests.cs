using LushThreads.Application.Services;
using LushThreads.Domain.Entites;

namespace LushThreads.Tests
{
    public class EmailTemplateServiceTests
    {
        private readonly EmailTemplateService _sut = new();

        [Fact]
        public void GenerateEmailConfirmationEmail_ContainsUserNameAndCode()
        {
            var user = new ApplicationUser { Name = "Mohamed", Email = "m@test.com" };

            var html = _sut.GenerateEmailConfirmationEmail(user, "123456");

            Assert.Contains("Mohamed", html);
            Assert.Contains("123456", html);
        }

        [Fact]
        public void GetOrderShippedEmail_ContainsOrderNumberAndLink()
        {
            var user = new ApplicationUser { Name = "Sara", Email = "s@test.com" };

            var html = _sut.GetOrderShippedEmail(user, 42, "https://example.com/order/42");

            Assert.Contains("Sara", html);
            Assert.Contains("42", html);
            Assert.Contains("https://example.com/order/42", html);
        }
    }
}
