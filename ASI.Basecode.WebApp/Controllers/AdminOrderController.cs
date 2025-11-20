using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using ASI.Basecode.WebApp.Attributes;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    [AdminOnly]  // Custom attribute handles admin authorization centrally
    public class AdminOrderController : ControllerBase<AdminOrderController>
    {
        private readonly IOrderService _orderService;

        public AdminOrderController(
            IOrderService orderService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// GET: AdminOrder/ManageOrders
        /// Displays all orders for admin management
        /// </summary>
        public IActionResult ManageOrders()
        {
            try
            {
                _logger.LogInformation("Admin accessing order management page");
                
                var orders = _orderService.GetAllOrders();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all orders for admin");
                TempData["ErrorMessage"] = "An error occurred while retrieving orders.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// POST: AdminOrder/UpdateStatus
        /// Updates the status of an order with proper validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            try
            {
                _logger.LogInformation("Admin attempting to update order {OrderId} status to {Status}", orderId, status);

                // Validate the status update using service layer
                var validation = _orderService.ValidateStatusUpdate(orderId, status);
                if (!validation.IsValid)
                {
                    TempData["ErrorMessage"] = validation.Message;
                    return RedirectToAction("OrderDetails", new { id = orderId });
                }

                // Update the status
                _orderService.UpdateOrderStatus(orderId, status);

                // Get the appropriate success message from service
                var successMessage = _orderService.GetStatusUpdateMessage(status);
                TempData["SuccessMessage"] = successMessage;

                _logger.LogInformation("Order {OrderId} status successfully updated to {Status}", orderId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order {OrderId} status to {Status}", orderId, status);
                TempData["ErrorMessage"] = "An error occurred while updating order status.";
            }

            return RedirectToAction("OrderDetails", new { id = orderId });
        }

        /// <summary>
        /// GET: AdminOrder/OrderDetails/5
        /// Displays detailed information about a specific order
        /// </summary>
        public IActionResult OrderDetails(int id)
        {
            try
            {
                _logger.LogInformation("Admin accessing order details for order {OrderId}", id);

                if (!_orderService.OrderExists(id))
                {
                    _logger.LogWarning("Admin attempted to access non-existent order {OrderId}", id);
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("ManageOrders");
                }

                var order = _orderService.GetOrderById(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("ManageOrders");
                }

                // Pass valid statuses to view for dropdown
                ViewBag.ValidStatuses = _orderService.GetValidOrderStatuses();

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order details for order {OrderId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving order details.";
                return RedirectToAction("ManageOrders");
            }
        }

        /// <summary>
        /// GET: AdminOrder/GetValidStatuses
        /// AJAX endpoint to get valid order statuses
        /// </summary>
        [HttpGet]
        public JsonResult GetValidStatuses()
        {
            try
            {
                var statuses = _orderService.GetValidOrderStatuses();
                return Json(new { success = true, statuses = statuses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving valid statuses");
                return Json(new { success = false, message = "Error retrieving statuses" });
            }
        }

        /// <summary>
        /// POST: AdminOrder/ValidateStatusUpdate
        /// AJAX endpoint to validate status update before submission
        /// </summary>
        [HttpPost]
        public JsonResult ValidateStatusUpdate(int orderId, string status)
        {
            try
            {
                var validation = _orderService.ValidateStatusUpdate(orderId, status);
                return Json(new { success = validation.IsValid, message = validation.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating status update for order {OrderId}", orderId);
                return Json(new { success = false, message = "Error validating status update" });
            }
        }
    }
}