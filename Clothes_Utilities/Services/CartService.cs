using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Cart;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service for shopping cart operations.
    /// Implements <see cref="ICartService"/>.
    /// </summary>
    public class CartService : ICartService
    {
        #region Fields

        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly IRepository<Domain.Entites.Product> _productRepository;
        private readonly IRepository<OrderHeader> _orderHeaderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<Stock> _stockRepository;
        private readonly IRepository<Domain.Entites.PaymentMethod> _paymentMethodRepository;
        private readonly ILogger<CartService> _logger;

        #endregion

        #region Constructor

        public CartService(
            IRepository<CartItem> cartItemRepository,
            IRepository<Domain.Entites.Product> productRepository,
            IRepository<OrderHeader> orderHeaderRepository,
            IRepository<OrderDetail> orderDetailRepository,
            IRepository<Stock> stockRepository,
            IRepository<Domain.Entites.PaymentMethod> paymentMethodRepository,
            ILogger<CartService> logger)
        {
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _stockRepository = stockRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _logger = logger;
        }

        #endregion

        #region Private Methods

        private (double Subtotal, double ShippingFee, double Tax, double Total) CalculateOrderTotals(IEnumerable<CartItem> cartItems)
        {
            double subtotal = (double)cartItems.Sum(i => i.Product.Product_Price * i.Quantity);
            double shippingFee = subtotal > 100 ? 0 : 10;
            double tax = subtotal * 0.08;
            double total = subtotal + shippingFee + tax;

            return (subtotal, shippingFee, tax, total);
        }

        private async Task<bool> ValidateStockAsync(int productId, string size, int quantity)
        {
            var stock = await _stockRepository.GetAsync(s => s.Product_Id == productId && s.Size == size);
            return stock != null && stock.Quantity >= quantity;
        }

        #endregion

        #region Cart Management

        public async Task<List<CartItem>> GetCartItemsAsync(string userId)
        {
            _logger.LogDebug("Retrieving cart items for user {UserId}.", userId);
            return await _cartItemRepository.GetAllAsync(
                filter: ci => ci.UserId == userId,
                includeProperties: "Product,Product.Stocks"
            );
        }

        public async Task<int> GetCartCountAsync(string userId)
        {
            var items = await _cartItemRepository.GetAllAsync(filter: ci => ci.UserId == userId);
            return items.Sum(i => i.Quantity);
        }

        public async Task<(bool success, string message, int cartCount)> AddToCartAsync(string userId, int productId, int quantity, string size)
        {
            _logger.LogInformation("Adding product {ProductId} to cart for user {UserId}.", productId, userId);

            var product = await _productRepository.GetAsync(p => p.Product_Id == productId, includeProperties: "Stocks");
            if (product == null)
                return (false, "Product not found", 0);

            // Validate size
            if (!string.IsNullOrEmpty(size))
            {
                var availableSize = product.Stocks?.Any(s => s.Size == size) ?? false;
                if (!availableSize)
                    return (false, "Selected size is not available", 0);
            }
            else
            {
                size = product.Stocks?.FirstOrDefault()?.Size;
                if (string.IsNullOrEmpty(size))
                    return (false, "No sizes available for this product", 0);
            }

            // Validate stock
            if (!await ValidateStockAsync(productId, size, quantity))
                return (false, "Insufficient stock for selected size", 0);

            var existingItem = await _cartItemRepository.GetAsync(ci => ci.ProductId == productId && ci.UserId == userId && ci.Size == size);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                await _cartItemRepository.UpdateAsync(existingItem);
            }
            else
            {
                await _cartItemRepository.CreateAsync(new CartItem
                {
                    ProductId = productId,
                    UserId = userId,
                    Quantity = quantity,
                    Size = size
                });
            }

            var cartCount = await GetCartCountAsync(userId);
            return (true, "Item added to cart successfully!", cartCount);
        }

        public async Task RemoveFromCartAsync(int cartItemId, string userId)
        {
            _logger.LogInformation("Removing cart item {CartItemId} for user {UserId}.", cartItemId, userId);
            var item = await _cartItemRepository.GetAsync(ci => ci.Id == cartItemId && ci.UserId == userId);
            if (item != null)
                await _cartItemRepository.DeleteAsync(item);
        }

        public async Task IncreaseQuantityAsync(int cartItemId, string userId)
        {
            _logger.LogInformation("Increasing quantity for cart item {CartItemId}.", cartItemId);
            var item = await _cartItemRepository.GetAsync(ci => ci.Id == cartItemId && ci.UserId == userId, includeProperties: "Product");
            if (item == null)
                return;

            item.Quantity += 1;
            await _cartItemRepository.UpdateAsync(item);
        }

        public async Task DecreaseQuantityAsync(int cartItemId, string userId)
        {
            _logger.LogInformation("Decreasing quantity for cart item {CartItemId}.", cartItemId);
            var item = await _cartItemRepository.GetAsync(ci => ci.Id == cartItemId && ci.UserId == userId, includeProperties: "Product");
            if (item == null)
                return;

            if (item.Quantity > 1)
            {
                item.Quantity -= 1;
                await _cartItemRepository.UpdateAsync(item);
            }
            else
            {
                await _cartItemRepository.DeleteAsync(item);
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            _logger.LogInformation("Clearing cart for user {UserId}.", userId);
            var items = await _cartItemRepository.GetAllAsync(filter: ci => ci.UserId == userId);
            if (items.Any())
                await _cartItemRepository.DeleteRangeAsync(items);
        }

        public async Task<(bool success, string message)> UpdateSizeAsync(int cartItemId, string userId, string newSize)
        {
            _logger.LogInformation("Updating size for cart item {CartItemId} to {NewSize}.", cartItemId, newSize);

            var cartItem = await _cartItemRepository.GetAsync(
                ci => ci.Id == cartItemId && ci.UserId == userId,
                includeProperties: "Product,Product.Stocks"
            );

            if (cartItem == null)
                return (false, $"Cart item not found.");

            if (string.IsNullOrEmpty(newSize))
                return (false, "Please select a valid size.");

            var availableSize = cartItem.Product.Stocks?.Any(s => s.Size == newSize) ?? false;
            if (!availableSize)
                return (false, $"Selected size '{newSize}' is not available.");

            var stock = cartItem.Product.Stocks.FirstOrDefault(s => s.Size == newSize);
            if (stock != null && stock.Quantity < cartItem.Quantity)
                return (false, $"Insufficient stock for size '{newSize}'. Only {stock.Quantity} items available.");

            // Check if another cart item with same product and new size exists
            var existingItem = await _cartItemRepository.GetAsync(ci =>
                ci.ProductId == cartItem.ProductId &&
                ci.UserId == userId &&
                ci.Size == newSize &&
                ci.Id != cartItemId);

            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
                await _cartItemRepository.UpdateAsync(existingItem);
                await _cartItemRepository.DeleteAsync(cartItem);
            }
            else
            {
                cartItem.Size = newSize;
                await _cartItemRepository.UpdateAsync(cartItem);
            }

            return (true, "Size updated successfully.");
        }

        #endregion

        #region Order Processing

        public async Task<CartViewModel> GetCartSummaryAsync(string userId)
        {
            _logger.LogDebug("Preparing cart summary for user {UserId}.", userId);

            var cartItems = await _cartItemRepository.GetAllAsync(
                filter: ci => ci.UserId == userId,
                includeProperties: "Product,Product.Stocks"
            );

            var (subtotal, shippingFee, tax, total) = CalculateOrderTotals(cartItems);

            return new CartViewModel
            {
                Items = cartItems,
                OrderHeader = new OrderHeader
                {
                    ApplicationUserId = userId,
                    OrderDate = DateTime.Now,
                    ShippingDate = DateTime.Now.AddDays(3),
                    Subtotal = subtotal,
                    ShippingFee = shippingFee,
                    Tax = tax,
                    OrderTotal = total,
                    OrderStatus = SD.StatusPending,
                    PaymentStatus = SD.PaymentStatusPending
                    // باقي الحقول ستملأ من الـ User في الـ Controller
                }
            };
        }

        public async Task<(bool success, string message, int orderId, string redirectUrl)> ProcessOrderAsync(
            string userId,
            CartViewModel viewModel,
            string selectedPaymentMethod,
            HttpRequest request)
        {
            _logger.LogInformation("Processing order for user {UserId}.", userId);

            var cartItems = await _cartItemRepository.GetAllAsync(
                filter: ci => ci.UserId == userId,
                includeProperties: "Product"
            );

            if (!cartItems.Any())
                return (false, "Cart is empty.", 0, null);

            // Recalculate totals to ensure consistency
            var (subtotal, shippingFee, tax, total) = CalculateOrderTotals(cartItems);

            // Create order header
            var orderHeader = new OrderHeader
            {
                ApplicationUserId = userId,
                OrderDate = DateTime.Now,
                ShippingDate = DateTime.Now.AddDays(3),
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                Tax = tax,
                OrderTotal = total,
                OrderStatus = SD.StatusPending,
                PaymentStatus = SD.PaymentStatusPending,
                Name = viewModel.OrderHeader.Name,
                PhoneNumber = viewModel.OrderHeader.PhoneNumber,
                StreetAddress = viewModel.OrderHeader.StreetAddress,
                Country = viewModel.OrderHeader.Country,
                PostalCode = viewModel.OrderHeader.PostalCode
            };

            await _orderHeaderRepository.CreateAsync(orderHeader);

            // Create order details
            foreach (var item in cartItems)
            {
                await _orderDetailRepository.CreateAsync(new OrderDetail
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = orderHeader.Id,
                    price = (double)item.Product.Product_Price,
                    Count = item.Quantity,
                    Size = item.Size
                });
            }

            // Handle payment method
            if (selectedPaymentMethod == "credit")
            {
                var domain = $"https://{request.Host.Value}";
                var lineItems = cartItems.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Product_Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Product_Name
                        }
                    },
                    Quantity = item.Quantity
                }).ToList();

                if (shippingFee > 0)
                {
                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(shippingFee * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Shipping Fee"
                            }
                        },
                        Quantity = 1
                    });
                }

                if (tax > 0)
                {
                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(tax * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Tax"
                            }
                        },
                        Quantity = 1
                    });
                }

                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"/Customer/Cart/OrderConfirmation/{orderHeader.Id}?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = domain + "/Customer/Cart/Index",
                    Mode = "payment",
                    LineItems = lineItems
                };

                var sessionService = new SessionService();
                Session session = sessionService.Create(options);

                orderHeader.SessionId = session.Id;
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                await _orderHeaderRepository.UpdateAsync(orderHeader);

                return (true, null, orderHeader.Id, session.Url);
            }
            else
            {
                // Cash on delivery
                await _cartItemRepository.DeleteRangeAsync(cartItems);
                return (true, null, orderHeader.Id, null);
            }
        }

        public async Task<(bool success, string message, OrderHeader orderHeader, List<OrderDetail> orderDetails)> ConfirmOrderAsync(
            int orderId,
            string sessionId,
            string userId)
        {
            _logger.LogInformation("Confirming order {OrderId} for user {UserId}.", orderId, userId);

            // Fetch order header without including ApplicationUser to avoid tracking conflicts
            var orderHeader = await _orderHeaderRepository.GetAsync(
                oh => oh.Id == orderId && oh.ApplicationUserId == userId);
            // No includeProperties

            if (orderHeader == null)
                return (false, "Order not found.", null, null);

            var orderDetails = await _orderDetailRepository.GetAllAsync(
                od => od.OrderHeaderId == orderId,
                includeProperties: "Product"
            );

            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    var sessionService = new SessionService();
                    var session = sessionService.Get(sessionId);

                    if (session.PaymentStatus == "paid")
                    {
                        // Update stock
                        foreach (var detail in orderDetails)
                        {
                            var stock = await _stockRepository.GetAsync(s => s.Product_Id == detail.ProductId && s.Size == detail.Size);
                            if (stock == null)
                                throw new Exception($"Stock not found for Product ID {detail.ProductId} with size {detail.Size}.");

                            if (stock.Quantity < detail.Count)
                                throw new Exception($"Insufficient stock for Product ID {detail.ProductId} (Size: {detail.Size}). Available: {stock.Quantity}, Requested: {detail.Count}.");

                            stock.Quantity -= detail.Count;
                            await _stockRepository.UpdateAsync(stock);
                        }

                        orderHeader.SessionId = sessionId;
                        orderHeader.PaymentIntentId = session.PaymentIntentId;
                        orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                        orderHeader.OrderStatus = SD.StatusApproved;

                        // Save payment method from Stripe (no change here)
                        var paymentIntentService = new PaymentIntentService();
                        var paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                        var options = new PaymentMethodGetOptions { Expand = new List<string> { "card" } };
                        var stripePaymentMethod = new PaymentMethodService().Get(paymentIntent.PaymentMethodId, options);

                        var paymentMethod = await _paymentMethodRepository.GetAsync(pm => pm.StripePaymentMethodId == paymentIntent.PaymentMethodId);
                        if (paymentMethod == null)
                        {
                            paymentMethod = new Domain.Entites.PaymentMethod
                            {
                                UserId = userId,
                                StripePaymentMethodId = paymentIntent.PaymentMethodId,
                                StripeCustomerId = session.CustomerId,
                                IsDefault = true,
                                CreatedAt = DateTime.UtcNow
                            };
                        }

                        if (stripePaymentMethod?.Card != null)
                        {
                            paymentMethod.CardBrand = stripePaymentMethod.Card.Brand;
                            paymentMethod.Last4 = stripePaymentMethod.Card.Last4;
                            paymentMethod.ExpMonth = (int)stripePaymentMethod.Card.ExpMonth;
                            paymentMethod.ExpYear = (int)stripePaymentMethod.Card.ExpYear;
                        }

                        if (stripePaymentMethod?.BillingDetails != null)
                        {
                            paymentMethod.BillingName = stripePaymentMethod.BillingDetails.Name;
                            paymentMethod.PhoneNumber = stripePaymentMethod.BillingDetails.Phone;
                            paymentMethod.AddressLine1 = stripePaymentMethod.BillingDetails.Address?.Line1;
                            paymentMethod.AddressLine2 = stripePaymentMethod.BillingDetails.Address?.Line2;
                            paymentMethod.City = stripePaymentMethod.BillingDetails.Address?.City;
                            paymentMethod.State = stripePaymentMethod.BillingDetails.Address?.State;
                            paymentMethod.PostalCode = stripePaymentMethod.BillingDetails.Address?.PostalCode;
                            paymentMethod.Country = stripePaymentMethod.BillingDetails.Address?.Country;
                        }

                        if (paymentMethod.Id == 0)
                            await _paymentMethodRepository.CreateAsync(paymentMethod);
                        else
                            await _paymentMethodRepository.UpdateAsync(paymentMethod);

                        // Update order header (ApplicationUser is null, no conflict)
                        await _orderHeaderRepository.UpdateAsync(orderHeader);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error confirming order {OrderId}.", orderId);
                    orderHeader.OrderStatus = "Failed";
                    orderHeader.PaymentStatus = "Failed";
                    await _orderHeaderRepository.UpdateAsync(orderHeader);
                    return (false, "Order failed due to payment processing error.", null, null);
                }
            }
            else
            {
                // Cash on delivery: order status remains pending
                await _orderHeaderRepository.UpdateAsync(orderHeader);
            }

            // Clear cart
            var cartItems = await _cartItemRepository.GetAllAsync(ci => ci.UserId == userId);
            if (cartItems.Any())
                await _cartItemRepository.DeleteRangeAsync(cartItems);

            return (true, null, orderHeader, orderDetails);
        }
        #endregion
    }
}