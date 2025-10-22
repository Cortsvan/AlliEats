using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
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
    public class CheckoutController : ControllerBase<CheckoutController>
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public CheckoutController(
            IOrderService orderService,
            ICartService cartService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        // GET: Checkout
        public IActionResult Index()
        {
            // Restrict admin access
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                TempData["ErrorMessage"] = "Admins cannot access checkout functionality.";
                return RedirectToAction("Index", "Home");
            }

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

                var checkoutModel = _orderService.PrepareCheckout(userId);
                return View(checkoutModel);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Checkout preparation failed: {Message}", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("ViewCart", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing checkout");
                TempData["ErrorMessage"] = "An error occurred while preparing checkout.";
                return RedirectToAction("ViewCart", "Cart");
            }
        }

        // POST: Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(CheckoutViewModel model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                return Json(new { success = false, message = "Admins cannot place orders." });
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("Model validation failed: {Errors}", errors);
                return Json(new { success = false, message = "Please fill in all required fields: " + errors });
            }

            try
            {
                // Get userId from session
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                _logger.LogInformation("Creating order for user: {UserId}", userId);
                var order = _orderService.CreateOrderFromCart(userId, model);
                _logger.LogInformation("Order created successfully: {OrderNumber}", order.OrderNumber);

                return Json(new
                {
                    success = true,
                    message = "Order placed successfully!",
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    redirectUrl = Url.Action("OrderConfirmation", "Checkout", new { id = order.Id })
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Order creation failed: {Message}", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detailed error: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return Json(new { success = false, message = $"Debug - Error: {ex.Message}" });
            }
        }

        // GET: Checkout/OrderConfirmation/5
        public IActionResult OrderConfirmation(int id)
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
                    _logger.LogWarning("Order not found: {OrderId}", id);
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Verify order belongs to current user (security check)
                if (order.UserId != userId)
                {
                    _logger.LogWarning("Access denied for order {OrderId} by user {UserId}", id, userId);
                    TempData["ErrorMessage"] = "Access denied.";
                    return RedirectToAction("Index", "Home");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order confirmation: {OrderId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving your order.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}