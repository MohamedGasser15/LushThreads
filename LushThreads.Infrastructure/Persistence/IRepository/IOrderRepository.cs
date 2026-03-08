using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    /// <summary>
    /// Repository interface for Order-specific operations.
    /// Inherits from generic IRepository for basic CRUD.
    /// </summary>
    public interface IOrderRepository : IRepository<OrderHeader>
    {
        /// <summary>
        /// Gets an order with its associated user.
        /// </summary>
        Task<OrderHeader?> GetOrderWithUserAsync(int orderId);

        /// <summary>
        /// Gets all orders with user information, ordered by newest first.
        /// </summary>
        Task<IEnumerable<OrderHeader>> GetAllOrdersWithUserAsync();

        /// <summary>
        /// Gets order details for a specific order.
        /// </summary>
        Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(int orderId);

        /// <summary>
        /// Gets order details with product information.
        /// </summary>
        Task<IEnumerable<OrderDetail>> GetOrderDetailsWithProductAsync(int orderId);

        /// <summary>
        /// Updates stock quantities based on order details.
        /// </summary>
        /// <param name="orderDetails">List of order details.</param>
        /// <param name="increase">True to increase stock (cancellation), false to decrease (approval).</param>
        Task UpdateStockForOrderDetailsAsync(IEnumerable<OrderDetail> orderDetails, bool increase);
    }
}
