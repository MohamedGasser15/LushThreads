using LushThreads.Application.Interfaces;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.ViewModels.Home;
using LushThreads.Domain.Entites;
using LushThreads.Web.Areas.Admin.Controllers;
using LushThreads.Web.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;

namespace LushThreads.Tests
{
    public class WebControllerTests
    {
        [Fact]
        public async Task HomeController_About_ReturnsViewWithZeroCartForAnonymous()
        {
            var homeServiceMock = new Mock<IHomeService>();
            var cartRepoMock = new Mock<IRepository<CartItem>>();
            var controller = new HomeController(NullLogger<HomeController>.Instance, homeServiceMock.Object, cartRepoMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.About();

            Assert.IsType<ViewResult>(result);
            Assert.Equal(0, controller.ViewBag.CartCount);
        }

        [Fact]
        public async Task HomeController_Shop_UsesDefaultPageSize18()
        {
            var homeServiceMock = new Mock<IHomeService>();
            homeServiceMock
                .Setup(s => s.GetShopProductsAsync(null, 1, 18))
                .ReturnsAsync((new List<ShopProductViewModel>(), 1, 0));
            var cartRepoMock = new Mock<IRepository<CartItem>>();
            var controller = new HomeController(NullLogger<HomeController>.Instance, homeServiceMock.Object, cartRepoMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.Shop();

            Assert.IsType<ViewResult>(result);
            Assert.Equal(18, controller.ViewBag.PageSize);
        }

        [Fact]
        public async Task BrandController_Index_ReturnsViewWithBrands()
        {
            var brandServiceMock = new Mock<IBrandService>();
            brandServiceMock.Setup(s => s.GetAllBrandsAsync()).ReturnsAsync(new List<Brand> { new() { Brand_Id = 1, Brand_Name = "Nike" } });
            var userManagerMock = TestHelpers.CreateUserManagerMock();
            var controller = new BrandController(brandServiceMock.Object, userManagerMock.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Brand>>(viewResult.Model);
            Assert.Single(model);
        }
    }
}
