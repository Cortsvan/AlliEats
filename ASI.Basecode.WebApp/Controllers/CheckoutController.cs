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
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    public class CheckoutController : ControllerBase<CheckoutController>
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IProfileService _profileService;
        private readonly IPaymentCardService _paymentCardService;

        public CheckoutController(
            IOrderService orderService,
            ICartService cartService,
            IProfileService profileService,
            IPaymentCardService paymentCardService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _orderService = orderService;
            _cartService = cartService;
            _profileService = profileService;
            _paymentCardService = paymentCardService;
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

                // Check if user has complete delivery information
                var userProfile = _profileService.GetProfile(userId);
                if (!HasCompleteDeliveryInformation(userProfile))
                {
                    _logger.LogInformation("User {UserId} attempted checkout without complete delivery information", userId);
                    TempData["ErrorMessage"] = "Please complete your delivery information before checkout.";
                    TempData["MissingDeliveryInfo"] = GetMissingDeliveryFields(userProfile);
                    return RedirectToAction("Edit", "Profile");
                }

                var checkoutModel = _orderService.PrepareCheckout(userId);

                // Add user profile information to checkout model for display
                checkoutModel.UserProfile = userProfile;

                // Load saved payment cards
                checkoutModel.SavedCards = _paymentCardService.GetCardsByUserId(userId).ToList();

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

            // Get userId from session
            var userId = HttpContext.Session.GetString("UserId")
                        ?? HttpContext.Session.GetString("UserName")
                        ?? User.Identity.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User session expired. Please login again." });
            }

            // Validate payment card selection for Credit or Debit Card payment method
            if (model.PaymentMethod == "Credit or Debit Card")
            {
                if (!model.SelectedCardId.HasValue || model.SelectedCardId.Value <= 0)
                {
                    _logger.LogWarning("User {UserId} attempted to place order with Credit/Debit Card but no card selected", userId);
                    return Json(new { 
                        success = false, 
                        message = "Please select a payment card or add a new card to continue.",
                        redirectAddCard = true,
                        addCardUrl = Url.Action("AddPaymentCard", "Settings", new { returnUrl = "/Checkout/Index" })
                    });
                }

                // Verify the card exists and belongs to the user
                try
                {
                    var selectedCard = _paymentCardService.GetCardById(model.SelectedCardId.Value);
                    if (selectedCard == null || selectedCard.UserId != userId)
                    {
                        _logger.LogWarning("User {UserId} attempted to use invalid card {CardId}", userId, model.SelectedCardId.Value);
                        return Json(new { success = false, message = "Invalid payment card selected." });
                    }

                    // Check if card is expired
                    if (selectedCard.IsExpired)
                    {
                        _logger.LogWarning("User {UserId} attempted to use expired card {CardId}", userId, model.SelectedCardId.Value);
                        return Json(new { success = false, message = "The selected payment card has expired. Please select a different card or add a new one." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating payment card {CardId} for user {UserId}", model.SelectedCardId.Value, userId);
                    return Json(new { success = false, message = "Error validating payment card." });
                }
            }

            // Double-check delivery information before placing order
            try
            {
                var userProfile = _profileService.GetProfile(userId);
                if (!HasCompleteDeliveryInformation(userProfile))
                {
                    _logger.LogWarning("User {UserId} attempted to place order without complete delivery information", userId);
                    return Json(new
                    {
                        success = false,
                        message = "Delivery information is incomplete. Please update your profile.",
                        redirectUrl = Url.Action("Edit", "Profile")
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking delivery information for user {UserId}", userId);
                return Json(new { success = false, message = "Error validating delivery information." });
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

        /// <summary>
        /// Checks if user has complete delivery information required for checkout
        /// </summary>
        /// <param name="profile">User profile</param>
        /// <returns>True if delivery information is complete</returns>
        private bool HasCompleteDeliveryInformation(ProfileViewModel profile)
        {
            // Required fields for delivery
            return !string.IsNullOrWhiteSpace(profile.Phone) &&
                   !string.IsNullOrWhiteSpace(profile.Address) &&
                   !string.IsNullOrWhiteSpace(profile.City);
        }

        /// <summary>
        /// Gets a list of missing delivery fields for error messaging
        /// </summary>
        /// <param name="profile">User profile</param>
        /// <returns>Comma-separated string of missing fields</returns>
        private string GetMissingDeliveryFields(ProfileViewModel profile)
        {
            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(profile.Phone))
                missingFields.Add("Phone Number");

            if (string.IsNullOrWhiteSpace(profile.Address))
                missingFields.Add("Address");

            if (string.IsNullOrWhiteSpace(profile.City))
                missingFields.Add("City");

            return string.Join(", ", missingFields);
        }
    }
}