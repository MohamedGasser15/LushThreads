using LushThreads.Application.Interfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace LushThreads.Tests
{
    public class CartServiceTests
    {
        private readonly Mock<ICartItemRepository> _cartRepoMock = new();
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IOrderRepository> _orderRepoMock = new();
        private readonly Mock<IOrderDetailRepository> _detailRepoMock = new();
        private readonly Mock<IStockRepository> _stockRepoMock = new();
        private readonly Mock<IPaymentMethodRepository> _paymentRepoMock = new();
        private readonly Mock<ILogger<CartService>> _loggerMock = new();
        private readonly CartService _sut;

        public CartServiceTests()
        {
            _sut = new CartService(
                _cartRepoMock.Object,
                _productRepoMock.Object,
                _orderRepoMock.Object,
                _detailRepoMock.Object,
                _stockRepoMock.Object,
                _paymentRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task AddToCartAsync_ReturnsFailure_WhenStockInsufficient()
        {
            var product = new Product
            {
                Product_Id = 1,
                Product_Name = "Hoodie",
                Product_Price = 50m,
                Stocks = new List<Stock> { new() { Product_Id = 1, Size = "M", Quantity = 10 } }
            };
            _productRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), null, false, default))
                .ReturnsAsync(product);
            _stockRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Stock, bool>>>(), null, false, default))
                .ReturnsAsync(new Stock { Product_Id = 1, Size = "M", Quantity = 1 });

            var (success, message, _) = await _sut.AddToCartAsync("u-1", 1, 5, "M");

            Assert.False(success);
            Assert.NotEmpty(message);
        }

        [Fact]
        public async Task AddToCartAsync_CreatesCartItem_WhenValid()
        {
            var product = new Product
            {
                Product_Id = 1,
                Product_Name = "Hoodie",
                Product_Price = 50m,
                Stocks = new List<Stock> { new() { Product_Id = 1, Size = "M", Quantity = 10 } }
            };
            _productRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>(), default))
                .ReturnsAsync(product);
            _stockRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Stock, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>(), default))
                .ReturnsAsync(new Stock { Product_Id = 1, Size = "M", Quantity = 10 });
            _cartRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<CartItem, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>(), default))
                .ReturnsAsync((CartItem)null!);
            _cartRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<CartItem, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>(), null, null, null, default))
                .ReturnsAsync(new List<CartItem> { new() { Quantity = 2 } });

            var (success, _, cartCount) = await _sut.AddToCartAsync("u-1", 1, 2, "M");

            Assert.True(success);
            Assert.Equal(2, cartCount);
            _cartRepoMock.Verify(r => r.CreateAsync(It.IsAny<CartItem>(), default), Times.Once);
        }

        [Fact]
        public async Task DecreaseQuantityAsync_DeletesItem_WhenQuantityIsOne()
        {
            var item = new CartItem { Id = 5, UserId = "u-1", Quantity = 1 };
            _cartRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<CartItem, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>(), default))
                .ReturnsAsync(item);

            await _sut.DecreaseQuantityAsync(5, "u-1");

            _cartRepoMock.Verify(r => r.DeleteAsync(item, default), Times.Once);
        }

        [Fact]
        public async Task UpdateSizeAsync_ReturnsFailure_WhenSizeNotAvailable()
        {
            var item = new CartItem
            {
                Id = 5,
                UserId = "u-1",
                Quantity = 1,
                Size = "M",
                Product = new Product
                {
                    Product_Id = 1,
                    Stocks = new List<Stock> { new() { Size = "M", Quantity = 10 } }
                }
            };

            _cartRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<CartItem, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>(), default))
                .ReturnsAsync(item);

            var (success, message) = await _sut.UpdateSizeAsync(5, "u-1", "XXL");

            Assert.False(success);
            Assert.Contains("not available", message);
        }

        [Fact]
        public async Task ClearCartAsync_DeletesAllUserItems()
        {
            var items = new List<CartItem> { new() { Id = 1 }, new() { Id = 2 } };
            _cartRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<CartItem, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>(), null, null, null, default))
                .ReturnsAsync(items);

            await _sut.ClearCartAsync("u-1");

            _cartRepoMock.Verify(r => r.DeleteRangeAsync(items, default), Times.Once);
        }
    }
}
