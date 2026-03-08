using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    public class OrderService : IOrderService
    {
        #region Fields

        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IAdminActivityService _adminActivityService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrderService> _logger;

        #endregion

        #region Constructor

        public OrderService(
            IOrderRepository orderRepository,
            IRepository<OrderDetail> orderDetailRepository,
            IAdminActivityService adminActivityService,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            UserManager<ApplicationUser> userManager,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _adminActivityService = adminActivityService;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _userManager = userManager;
            _logger = logger;
        }

        #endregion

        #region Private Methods

        private string GenerateOrderLink(ApplicationUser user, int orderId)
        {
            return $"/Profile/Orders?userId={user.Id}";
        }

        #endregion

        #region Public Methods

        public async Task<IEnumerable<OrderHeader>> GetAllOrdersAsync()
        {
            _logger.LogDebug("Retrieving all orders with user information.");
            return await _orderRepository.GetAllOrdersWithUserAsync();
        }

        public async Task<OrderVM> GetOrderDetailsAsync(int orderId)
        {
            _logger.LogDebug("Retrieving order details for order {OrderId}.", orderId);

            var orderHeader = await _orderRepository.GetOrderWithUserAsync(orderId);
            if (orderHeader == null)
                return null;

            var orderDetails = await _orderRepository.GetOrderDetailsWithProductAsync(orderId);

            return new OrderVM
            {
                OrderHeader = orderHeader,
                OrderDetails = orderDetails
            };
        }

        public async Task UpdateCustomerInfoAsync(int orderId, string name, string phoneNumber, string userId, string ipAddress)
        {
            _logger.LogInformation("Updating customer info for order {OrderId} by user {UserId}.", orderId, userId);

            var order = await _orderRepository.GetAsync(o => o.Id == orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            order.Name = name;
            order.PhoneNumber = phoneNumber;

            await _orderRepository.UpdateAsync(order);
            await _adminActivityService.LogActivityAsync(userId, "UpdateCustomerInfo", $"Update Customer Info (order: #{order.Id})", ipAddress);
        }

        public async Task UpdateShippingInfoAsync(int orderId, string streetAddress, string country, string postalCode, string userId, string ipAddress)
        {
            _logger.LogInformation("Updating shipping info for order {OrderId} by user {UserId}.", orderId, userId);

            var order = await _orderRepository.GetAsync(o => o.Id == orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            order.StreetAddress = streetAddress;
            order.Country = country;
            order.PostalCode = postalCode;

            await _orderRepository.UpdateAsync(order);
            await _adminActivityService.LogActivityAsync(userId, "UpdateShippingInfo", $"Update Shipping Info (order: #{order.Id})", ipAddress);
        }

        public async Task UpdateTrackingInfoAsync(int orderId, string trackingNumber, string carrier, string userId, string ipAddress)
        {
            _logger.LogInformation("Updating tracking info for order {OrderId} by user {UserId}.", orderId, userId);

            var order = await _orderRepository.GetAsync(o => o.Id == orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            order.TrackingNumber = trackingNumber;
            order.Carrier = carrier;

            await _orderRepository.UpdateAsync(order);
            await _adminActivityService.LogActivityAsync(userId, "UpdateTrackingInfo", $"Update Tracking Info (order: #{order.Id})", ipAddress);
        }

        public async Task ApproveOrderAsync(int orderId, string userId, string ipAddress)
        {
            _logger.LogInformation("Approving order {OrderId} by user {UserId}.", orderId, userId);

            var order = await _orderRepository.GetOrderWithUserAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            if (order.OrderStatus != SD.StatusPending && order.OrderStatus != SD.StatusApproved)
                throw new InvalidOperationException($"Order cannot be approved from status {order.OrderStatus}.");

            var orderDetails = await _orderRepository.GetOrderDetailsWithProductAsync(orderId);

            // Update stock (decrease)
            await _orderRepository.UpdateStockForOrderDetailsAsync(orderDetails, increase: false);

            order.OrderStatus = SD.StatusApproved;
            order.PaymentStatus = SD.PaymentStatusApproved;
            order.PaymentDate = DateTime.Now;

            await _orderRepository.UpdateAsync(order);
            await _adminActivityService.LogActivityAsync(userId, "OrderApproval", $"Approved Order (order: #{order.Id})", ipAddress);

            // Send email
            if (order.ApplicationUser != null)
            {
                var emailBody = _emailTemplateService.GetOrderInProcessEmail(order.ApplicationUser, order.Id, GenerateOrderLink(order.ApplicationUser, order.Id));
                await _emailSender.SendEmailAsync(order.ApplicationUser.Email, "Your Order Has Been Approved", emailBody);
            }
        }

        public async Task CancelOrderAsync(int orderId, string userId, string ipAddress)
        {
            _logger.LogInformation("Cancelling order {OrderId} by user {UserId}.", orderId, userId);

            var order = await _orderRepository.GetOrderWithUserAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            if (order.OrderStatus != SD.StatusPending && order.OrderStatus != SD.StatusApproved)
                throw new InvalidOperationException($"Cannot cancel order with status: {order.OrderStatus}.");

            var orderDetails = await _orderRepository.GetOrderDetailsWithProductAsync(orderId);

            // If order was approved, restore stock
            if (order.OrderStatus == SD.StatusApproved)
            {
                await _orderRepository.UpdateStockForOrderDetailsAsync(orderDetails, increase: true);
            }

            // Process refund if payment was made
            if (!string.IsNullOrEmpty(order.PaymentIntentId))
            {
                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = order.PaymentIntentId,
                    Reason = RefundReasons.RequestedByCustomer
                };

                try
                {
                    refundService.Create(refundOptions);
                    order.PaymentStatus = SD.StatusRefunded;
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Refund failed for order {OrderId}.", orderId);
                    throw new InvalidOperationException("Refund failed. Please check Stripe dashboard.", ex);
                }
            }

            order.OrderStatus = SD.StatusCancelled;

            await _orderRepository.UpdateAsync(order);
            await _adminActivityService.LogActivityAsync(userId, "OrderCancellation", $"Cancelled Order (order: #{order.Id})", ipAddress);

            // Send email
            if (order.ApplicationUser != null)
            {
                var emailBody = _emailTemplateService.GetOrderCancelledEmail(order.ApplicationUser, order.Id, GenerateOrderLink(order.ApplicationUser, order.Id));
                await _emailSender.SendEmailAsync(order.ApplicationUser.Email, "Your Order Has Been Cancelled", emailBody);
            }
        }

        public async Task ShipOrderAsync(int orderId, string userId, string ipAddress)
        {
            _logger.LogInformation("Shipping order {OrderId} by user {UserId}.", orderId, userId);

            var order = await _orderRepository.GetOrderWithUserAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            if (order.OrderStatus != SD.StatusApproved)
                throw new InvalidOperationException("Only approved orders can be shipped.");

            order.OrderStatus = SD.StatusShipped;
            order.ShippingDate = DateTime.Now;

            await _orderRepository.UpdateAsync(order);
            await _adminActivityService.LogActivityAsync(userId, "OrderShipped", $"Shipped Order (order: #{order.Id})", ipAddress);

            // Send email
            if (order.ApplicationUser != null)
            {
                var emailBody = _emailTemplateService.GetOrderShippedEmail(order.ApplicationUser, order.Id, GenerateOrderLink(order.ApplicationUser, order.Id));
                await _emailSender.SendEmailAsync(order.ApplicationUser.Email, "Your Order Has Been Shipped", emailBody);
            }
        }

        public async Task DeliverOrderAsync(int orderId, string userId, string ipAddress)
        {
            _logger.LogInformation("Marking order {OrderId} as delivered by user {UserId}.", orderId, userId);

            var order = await _orderRepository.GetOrderWithUserAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            if (order.OrderStatus != SD.StatusShipped)
                throw new InvalidOperationException("Cannot mark as delivered unless order is shipped.");

            order.OrderStatus = SD.StatusDelivered;

            await _orderRepository.UpdateAsync(order);
            await _adminActivityService.LogActivityAsync(userId, "OrderDelivered", $"Delivered Order (order: #{order.Id})", ipAddress);

            // Send email
            if (order.ApplicationUser != null)
            {
                var emailBody = _emailTemplateService.GetOrderDeliveredEmail(order.ApplicationUser, order.Id, GenerateOrderLink(order.ApplicationUser, order.Id));
                await _emailSender.SendEmailAsync(order.ApplicationUser.Email, "Your Order Has Been Delivered", emailBody);
            }
        }

        public async Task RemoveCanceledOrRefundedOrderAsync(int orderId, string returnUrl)
        {
            _logger.LogInformation("Removing cancelled/refunded order {OrderId}.", orderId);

            var order = await _orderRepository.GetAsync(o => o.Id == orderId &&
                (o.OrderStatus == SD.StatusCancelled || o.OrderStatus == SD.StatusRefunded));

            if (order == null)
                throw new InvalidOperationException("Order not found or not eligible for removal.");

            // حذف تفاصيل الطلب أولاً
            var orderDetails = await _orderRepository.GetOrderDetailsAsync(orderId);
            if (orderDetails.Any())
            {
                await _orderDetailRepository.DeleteRangeAsync(orderDetails);
            }

            // ثم حذف الطلب نفسه
            await _orderRepository.DeleteAsync(order);
            _logger.LogInformation("Order {OrderId} removed successfully.", orderId);
        }

        #endregion
    }
}