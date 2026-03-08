using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Service interface for order management operations.
    /// </summary>
    public interface IOrderService
    {
        #region Query Methods

        /// <summary>
        /// Retrieves all orders with user information.
        /// </summary>
        Task<IEnumerable<OrderHeader>> GetAllOrdersAsync();

        /// <summary>
        /// Retrieves order details for a specific order.
        /// </summary>
        Task<OrderVM> GetOrderDetailsAsync(int orderId);

        #endregion

        #region Command Methods

        /// <summary>
        /// Updates customer information (name, phone).
        /// </summary>
        Task UpdateCustomerInfoAsync(int orderId, string name, string phoneNumber, string userId, string ipAddress);

        /// <summary>
        /// Updates shipping information (address, country, postal code).
        /// </summary>
        Task UpdateShippingInfoAsync(int orderId, string streetAddress, string country, string postalCode, string userId, string ipAddress);

        /// <summary>
        /// Updates tracking information (tracking number, carrier).
        /// </summary>
        Task UpdateTrackingInfoAsync(int orderId, string trackingNumber, string carrier, string userId, string ipAddress);

        /// <summary>
        /// Approves an order, updates stock, and sends email.
        /// </summary>
        Task ApproveOrderAsync(int orderId, string userId, string ipAddress);

        /// <summary>
        /// Cancels an order, restores stock, processes refund, and sends email.
        /// </summary>
        Task CancelOrderAsync(int orderId, string userId, string ipAddress);

        /// <summary>
        /// Marks an order as shipped and sends email.
        /// </summary>
        Task ShipOrderAsync(int orderId, string userId, string ipAddress);

        /// <summary>
        /// Marks an order as delivered and sends email.
        /// </summary>
        Task DeliverOrderAsync(int orderId, string userId, string ipAddress);

        /// <summary>
        /// Removes cancelled or refunded orders.
        /// </summary>
        Task RemoveCanceledOrRefundedOrderAsync(int orderId, string returnUrl);

        #endregion
    }
}
