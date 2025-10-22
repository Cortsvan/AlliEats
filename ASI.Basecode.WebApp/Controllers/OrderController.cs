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

        // GET: Order/MyOrders
        public IActionResult MyOrders()
        {
            // Restrict admin access
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                TempData["ErrorMessage"] = "Admins cannot access order history.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Get userId from session
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                _logger.LogInformation("MyOrders called for user: {UserId}", userId);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No userId found in session");
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                var orders = _orderService.GetOrdersByUserId(userId);
                _logger.LogInformation("Retrieved {OrderCount} orders for user {UserId}", orders?.Count() ?? 0, userId);

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving orders");
                TempData["ErrorMessage"] = $"An error occurred while retrieving your orders: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Order/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                // Get userId from session
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                var order = _orderService.GetOrderById(id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("MyOrders");
                }

                // Verify order belongs to current user (security check)
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin" && order.UserId != userId)
                {
                    TempData["ErrorMessage"] = "Access denied.";
                    return RedirectToAction("MyOrders");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order details");
                TempData["ErrorMessage"] = "An error occurred while retrieving order details.";
                return RedirectToAction("MyOrders");
            }
        }
    }
}