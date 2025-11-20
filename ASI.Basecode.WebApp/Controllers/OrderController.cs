using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Attributes;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    [CustomerOnly] // Custom attribute handles customer-only access
    public class OrderController : ControllerBase<OrderController>
    {
        private readonly IOrderService _orderService;

        public OrderController(
            IOrderService orderService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// GET: Order/MyOrders
        /// Displays the customer's order history
        /// </summary>
        public IActionResult MyOrders()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No userId found in session for MyOrders request");
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("MyOrders called for user: {UserId}", userId);

                var orders = _orderService.GetOrdersByUserId(userId);
                _logger.LogInformation("Retrieved {OrderCount} orders for user {UserId}", orders?.Count() ?? 0, userId);

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving orders for user");
                TempData["ErrorMessage"] = "An error occurred while retrieving your orders.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// GET: Order/Details/5
        /// Displays detailed information about a specific order
        /// </summary>
        public IActionResult Details(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("User {UserId} requesting order details for order {OrderId}", userId, id);

                if (!_orderService.OrderExists(id))
                {
                    _logger.LogWarning("User {UserId} attempted to access non-existent order {OrderId}", userId, id);
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("MyOrders");
                }

                // Validate ownership using service layer
                if (!_orderService.ValidateOrderOwnership(id, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to access order {OrderId} they don't own", userId, id);
                    TempData["ErrorMessage"] = "Access denied.";
                    return RedirectToAction("MyOrders");
                }

                var order = _orderService.GetOrderById(id);
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order details for order {OrderId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving order details.";
                return RedirectToAction("MyOrders");
            }
        }

        /// <summary>
        /// POST: Order/ConfirmReceipt
        /// AJAX endpoint for customers to confirm receipt of their order
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmReceipt(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                _logger.LogInformation("User {UserId} attempting to confirm receipt of order {OrderId}", userId, orderId);

                // Validate receipt confirmation using service layer
                var validation = _orderService.ValidateReceiptConfirmation(orderId, userId);
                if (!validation.CanConfirm)
                {
                    _logger.LogWarning("Receipt confirmation validation failed for order {OrderId} by user {UserId}: {Message}",
                        orderId, userId, validation.Message);
                    return Json(new { success = false, message = validation.Message });
                }

                // Update order status to "Received"
                _orderService.UpdateOrderStatus(orderId, "Received");

                _logger.LogInformation("Order {OrderId} receipt confirmed successfully by user {UserId}", orderId, userId);

                return Json(new
                {
                    success = true,
                    message = "Order receipt confirmed successfully! Thank you for your order."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while confirming order receipt for order {OrderId}", orderId);
                return Json(new { success = false, message = "An error occurred while confirming receipt." });
            }
        }

        /// <summary>
        /// POST: Order/CancelOrder
        /// AJAX endpoint for customers to cancel their pending orders
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                _logger.LogInformation("User {UserId} attempting to cancel order {OrderId}", userId, orderId);

                // Validate order cancellation using service layer
                var validation = _orderService.ValidateOrderCancellation(orderId, userId);
                if (!validation.CanCancel)
                {
                    _logger.LogWarning("Order cancellation validation failed for order {OrderId} by user {UserId}: {Message}",
                        orderId, userId, validation.Message);
                    return Json(new { success = false, message = validation.Message });
                }

                // Cancel the order
                _orderService.CancelOrder(orderId);
                _logger.LogInformation("Order {OrderId} cancelled successfully by user {UserId}", orderId, userId);

                return Json(new
                {
                    success = true,
                    message = "Order has been cancelled successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling order {OrderId} for user {UserId}",
                    orderId, GetCurrentUserId());
                return Json(new { success = false, message = "An error occurred while cancelling the order." });
            }
        }

        /// <summary>
        /// GET: Order/TrackOrder
        /// Allows customers to track their orders by order number
        /// </summary>
        public IActionResult TrackOrder()
        {
            return View();
        }

        /// <summary>
        /// POST: Order/TrackOrder
        /// Processes order tracking by order number
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TrackOrder(string orderNumber)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                if (string.IsNullOrWhiteSpace(orderNumber))
                {
                    ViewBag.ErrorMessage = "Please enter a valid order number.";
                    return View();
                }

                _logger.LogInformation("User {UserId} tracking order number {OrderNumber}", userId, orderNumber);

                var order = _orderService.GetOrderByOrderNumber(orderNumber.Trim());

                if (order == null)
                {
                    ViewBag.ErrorMessage = "Order not found. Please check your order number and try again.";
                    return View();
                }

                // Validate ownership
                if (!_orderService.ValidateOrderOwnership(order.Id, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to track order {OrderNumber} they don't own",
                        userId, orderNumber);
                    ViewBag.ErrorMessage = "Access denied. This order does not belong to you.";
                    return View();
                }

                _logger.LogInformation("Order {OrderNumber} found and tracked successfully by user {UserId}",
                    orderNumber, userId);

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while tracking order number {OrderNumber}", orderNumber);
                ViewBag.ErrorMessage = "An error occurred while tracking your order. Please try again.";
                return View();
            }
        }

        /// <summary>
        /// GET: Order/GetOrderStatus/{id}
        /// AJAX endpoint to get real-time order status
        /// </summary>
        [HttpGet]
        public IActionResult GetOrderStatus(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired." });
                }

                if (!_orderService.ValidateOrderOwnership(id, userId))
                {
                    return Json(new { success = false, message = "Access denied." });
                }

                var order = _orderService.GetOrderById(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                return Json(new
                {
                    success = true,
                    status = order.Status,
                    lastUpdated = order.UpdatedTime?.ToString("MMM dd, yyyy h:mm tt") ??
                                  order.CreatedTime.ToString("MMM dd, yyyy h:mm tt"),
                    canConfirmReceipt = order.Status == "On the Way",
                    canCancel = order.Status == "Pending"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order status for order {OrderId}", id);
                return Json(new { success = false, message = "Error retrieving order status." });
            }
        }

        /// <summary>
        /// Helper method to get current user ID from multiple sources
        /// </summary>
        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId")
                   ?? HttpContext.Session.GetString("UserName")
                   ?? User.Identity.Name;
        }
    }
}