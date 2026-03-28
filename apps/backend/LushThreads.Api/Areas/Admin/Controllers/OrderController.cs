using AutoMapper;
using LushThreads.Application.DTOs.Order;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LushThreads.Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = SD.Admin)]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public OrderController(
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _orderService = orderService;
            _userManager = userManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderHeaderDto>>> GetOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var orderDtos = _mapper.Map<IEnumerable<OrderHeaderDto>>(orders);
            return Ok(orderDtos);
        }

        /// <summary>
        /// Get order details by id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailsResponseDto>> GetOrder(int id)
        {
            var orderVm = await _orderService.GetOrderDetailsAsync(id);
            if (orderVm?.OrderHeader == null)
                return NotFound();

            var orderDto = _mapper.Map<OrderDetailsResponseDto>(orderVm);
            return Ok(orderDto);
        }

        /// <summary>
        /// Update customer information
        /// </summary>
        [HttpPut("{id}/customer-info")]
        public async Task<IActionResult> UpdateCustomerInfo(int id, [FromBody] UpdateCustomerInfoRequestDto request)
        {
            if (id != request.OrderId)
                return BadRequest("Order ID mismatch");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _orderService.UpdateCustomerInfoAsync(
                    request.OrderId,
                    request.Name,
                    request.PhoneNumber,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update shipping information
        /// </summary>
        [HttpPut("{id}/shipping-info")]
        public async Task<IActionResult> UpdateShippingInfo(int id, [FromBody] UpdateShippingInfoRequestDto request)
        {
            if (id != request.OrderId)
                return BadRequest("Order ID mismatch");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _orderService.UpdateShippingInfoAsync(
                    request.OrderId,
                    request.StreetAddress,
                    request.Country,
                    request.PostalCode,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update tracking information
        /// </summary>
        [HttpPut("{id}/tracking")]
        public async Task<IActionResult> UpdateTrackingInfo(int id, [FromBody] UpdateTrackingInfoRequestDto request)
        {
            if (id != request.OrderId)
                return BadRequest("Order ID mismatch");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _orderService.UpdateTrackingInfoAsync(
                    request.OrderId,
                    request.TrackingNumber,
                    request.Carrier,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Approve an order
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _orderService.ApproveOrderAsync(
                    id,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
                return Ok(new { message = $"Order #{id} approved successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _orderService.CancelOrderAsync(
                    id,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
                return Ok(new { message = $"Order #{id} cancelled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark order as shipped
        /// </summary>
        [HttpPost("{id}/ship")]
        public async Task<IActionResult> ShipOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _orderService.ShipOrderAsync(
                    id,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
                return Ok(new { message = $"Order #{id} marked as shipped." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark order as delivered
        /// </summary>
        [HttpPost("{id}/deliver")]
        public async Task<IActionResult> DeliverOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _orderService.DeliverOrderAsync(
                    id,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
                return Ok(new { message = $"Order #{id} marked as delivered." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove cancelled/refunded order
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _orderService.RemoveCanceledOrRefundedOrderAsync(id, null);
                return Ok(new { message = $"Order #{id} removed successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
    }
}
