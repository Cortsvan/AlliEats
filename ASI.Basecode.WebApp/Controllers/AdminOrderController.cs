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
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    [AdminOnly] 
    public class AdminOrderController : ControllerBase<AdminOrderController>
    {
        private readonly IOrderService _orderService;
        private readonly IEmailNotificationService _emailNotificationService;

        public AdminOrderController(
            IOrderService orderService,
            IEmailNotificationService emailNotificationService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _orderService = orderService;
            _emailNotificationService = emailNotificationService;
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
        /// Updates the status of an order with proper validation and sends email notification
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            try
            {
                _logger.LogInformation("Admin attempting to update order {OrderId} status to {Status}", orderId, status);

                var currentOrder = _orderService.GetOrderById(orderId);
                if (currentOrder == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("ManageOrders");
                }

                var oldStatus = currentOrder.Status;

                var validation = _orderService.ValidateStatusUpdate(orderId, status);
                if (!validation.IsValid)
                {
                    TempData["ErrorMessage"] = validation.Message;
                    return RedirectToAction("OrderDetails", new { id = orderId });
                }

                _orderService.UpdateOrderStatus(orderId, status);

                try
                {
                    var customerInfo = _orderService.GetOrderCustomerInfo(orderId);

                    bool emailSent = await SendStatusUpdateEmail(customerInfo.Email, customerInfo.Name, currentOrder.OrderNumber, oldStatus, status);

                    if (!emailSent)
                    {
                        _logger.LogWarning("Email notification failed for order {OrderId} status update", orderId);    
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Error sending email notification for order {OrderId} status update", orderId);
                    
                }

                var successMessage = _orderService.GetStatusUpdateMessage(status);
                TempData["SuccessMessage"] = successMessage;

                _logger.LogInformation("Order {OrderId} status successfully updated from {OldStatus} to {NewStatus}", orderId, oldStatus, status);
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

        /// <summary>
        /// Sends appropriate email notification based on order status
        /// </summary>
        private async Task<bool> SendStatusUpdateEmail(string email, string customerName, string orderNumber, string oldStatus, string newStatus)
        {
            try
            {
                return newStatus switch
                {
                    "Ready" => await _emailNotificationService.SendOrderReadyNotificationEmailAsync(email, customerName, orderNumber),
                    "On the Way" => await _emailNotificationService.SendOrderDeliveredNotificationEmailAsync(email, customerName, orderNumber),
                    "Cancelled" => await _emailNotificationService.SendOrderCancelledNotificationEmailAsync(email, customerName, orderNumber, "Order cancelled by restaurant"),
                    _ => await _emailNotificationService.SendOrderStatusUpdateEmailAsync(email, customerName, orderNumber, oldStatus, newStatus)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status update email for order {OrderNumber}", orderNumber);
                return false;
            }
        }

    }
}