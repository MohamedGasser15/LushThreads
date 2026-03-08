using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Cart;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Service interface for shopping cart operations.
    /// </summary>
    public interface ICartService
    {
        #region Cart Management

        /// <summary>
        /// Gets the current user's cart items with product details.
        /// </summary>
        Task<List<CartItem>> GetCartItemsAsync(string userId);

        /// <summary>
        /// Gets the cart count (total quantity) for the user.
        /// </summary>
        Task<int> GetCartCountAsync(string userId);

        /// <summary>
        /// Adds an item to the cart or updates quantity if exists.
        /// </summary>
        Task<(bool success, string message, int cartCount)> AddToCartAsync(string userId, int productId, int quantity, string size);

        /// <summary>
        /// Removes an item from the cart.
        /// </summary>
        Task RemoveFromCartAsync(int cartItemId, string userId);

        /// <summary>
        /// Increases quantity of a cart item.
        /// </summary>
        Task IncreaseQuantityAsync(int cartItemId, string userId);

        /// <summary>
        /// Decreases quantity of a cart item; removes if quantity becomes 0.
        /// </summary>
        Task DecreaseQuantityAsync(int cartItemId, string userId);

        /// <summary>
        /// Clears all items from the user's cart.
        /// </summary>
        Task ClearCartAsync(string userId);

        /// <summary>
        /// Updates the size of a cart item.
        /// </summary>
        Task<(bool success, string message)> UpdateSizeAsync(int cartItemId, string userId, string newSize);

        #endregion

        #region Order Processing

        /// <summary>
        /// Gets the cart summary view model with order header prepopulated.
        /// </summary>
        Task<CartViewModel> GetCartSummaryAsync(string userId);

        /// <summary>
        /// Processes the order summary (creates order, handles payment, clears cart).
        /// </summary>
        Task<(bool success, string message, int orderId, string redirectUrl)> ProcessOrderAsync(
            string userId,
            CartViewModel viewModel,
            string selectedPaymentMethod,
            HttpRequest request);

        /// <summary>
        /// Confirms an order after payment (updates stock, payment status, clears cart).
        /// </summary>
        Task<(bool success, string message, OrderHeader orderHeader, List<OrderDetail> orderDetails)> ConfirmOrderAsync(
            int orderId,
            string sessionId,
            string userId);

        #endregion
    }
}
