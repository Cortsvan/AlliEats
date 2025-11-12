using ASI.Basecode.Services.Interfaces;
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

        // GET: AdminOrder/ManageOrders
        public IActionResult ManageOrders()
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var orders = _orderService.GetAllOrders().OrderByDescending(o => o.CreatedTime);
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all orders");
                TempData["ErrorMessage"] = "An error occurred while retrieving orders.";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: AdminOrder/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var validStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "On the Way", "Received", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    TempData["ErrorMessage"] = "Invalid status selected.";
                    return RedirectToAction("ManageOrders");
                }

                _orderService.UpdateOrderStatus(orderId, status);
                TempData["SuccessMessage"] = $"Order status updated to {status} successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order status");
                TempData["ErrorMessage"] = "An error occurred while updating order status.";
            }

            return RedirectToAction("ManageOrders");
        }

        // GET: AdminOrder/OrderDetails/5
        public IActionResult OrderDetails(int id)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var order = _orderService.GetOrderById(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("ManageOrders");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order details");
                TempData["ErrorMessage"] = "An error occurred while retrieving order details.";
                return RedirectToAction("ManageOrders");
            }
        }
    }
}