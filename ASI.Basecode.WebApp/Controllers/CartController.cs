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
    public class CartController : ControllerBase<CartController>
    {
        private readonly ICartService _cartService;
        private readonly IMenuService _menuService;

        public CartController(
            ICartService cartService,
            IMenuService menuService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _cartService = cartService;
            _menuService = menuService;
        }

        /// <summary>
        /// GET: Cart/ViewCart
        /// Displays the user's shopping cart
        /// </summary>
        public IActionResult ViewCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("Displaying cart for user {UserId}", userId);

                var cart = _cartService.GetCartByUserId(userId);
                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while displaying cart");
                TempData["ErrorMessage"] = "An error occurred while loading your cart.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// POST: Cart/AddItem
        /// Adds an item to the user's cart
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddItem(int menuItemId, int quantity = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                _logger.LogInformation("User {UserId} adding item {MenuItemId} (quantity: {Quantity}) to cart", userId, menuItemId, quantity);

                _cartService.AddToCart(userId, menuItemId, quantity);

                var itemCount = _cartService.GetCartItemCount(userId);
                _logger.LogInformation("Item successfully added to cart. New cart count: {ItemCount}", itemCount);

                return Json(new { success = true, message = "Item added to cart!", itemCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item {MenuItemId} to cart", menuItemId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: Cart/UpdateItem
        /// Updates the quantity of a cart item
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateItem(int cartItemId, int quantity)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                // Validate ownership
                if (!_cartService.ValidateCartItemOwnership(cartItemId, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to update cart item {CartItemId} they don't own", userId, cartItemId);
                    TempData["ErrorMessage"] = "Access denied.";
                    return RedirectToAction("ViewCart");
                }

                _logger.LogInformation("User {UserId} updating cart item {CartItemId} to quantity {Quantity}", userId, cartItemId, quantity);

                _cartService.UpdateCartItem(cartItemId, quantity);
                TempData["SuccessMessage"] = "Cart updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {CartItemId}", cartItemId);
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("ViewCart");
        }

        /// <summary>
        /// POST: Cart/RemoveItem
        /// Removes an item from the cart
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int cartItemId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return HandleResponse(false, "User session expired. Please login again.");
                }

                // Validate ownership
                if (!_cartService.ValidateCartItemOwnership(cartItemId, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to remove cart item {CartItemId} they don't own", userId, cartItemId);
                    return HandleResponse(false, "Access denied.");
                }

                _logger.LogInformation("User {UserId} removing cart item {CartItemId}", userId, cartItemId);

                _cartService.RemoveFromCart(cartItemId);

                return HandleResponse(true, "Item removed from cart!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {CartItemId}", cartItemId);
                return HandleResponse(false, ex.Message);
            }
        }

        /// <summary>
        /// POST: Cart/Clear
        /// Clears all items from the cart
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return HandleResponse(false, "User session expired. Please login again.");
                }

                _logger.LogInformation("User {UserId} clearing entire cart", userId);

                _cartService.ClearCart(userId);

                return HandleResponse(true, "Cart cleared successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", GetCurrentUserId());
                return HandleResponse(false, ex.Message);
            }
        }

        /// <summary>
        /// POST: Cart/CheckStockAvailability
        /// AJAX endpoint to validate stock availability for cart items
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckStockAvailability()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                _logger.LogInformation("Checking stock availability for user {UserId}", userId);

                var validationResult = _cartService.ValidateCartStock(userId);

                return Json(new
                {
                    success = true,
                    hasIssues = validationResult.HasIssues,
                    stockIssues = validationResult.StockIssues.Select(issue => new
                    {
                        cartItemId = issue.CartItemId,
                        menuItemId = issue.MenuItemId,
                        itemName = issue.ItemName,
                        requestedQuantity = issue.RequestedQuantity,
                        availableStock = issue.AvailableStock,
                        issue = issue.Issue,
                        message = issue.Message
                    }),
                    message = validationResult.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock availability");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: Cart/AutoFixStockIssues
        /// AJAX endpoint to automatically fix stock issues in the cart
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AutoFixStockIssues()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                _logger.LogInformation("Auto-fixing stock issues for user {UserId}", userId);

                var fixResult = _cartService.AutoFixStockIssues(userId);

                return Json(new
                {
                    success = fixResult.Success,
                    message = fixResult.Message,
                    fixedItems = fixResult.FixedItems.Select(item => new
                    {
                        name = item.Name,
                        oldQuantity = item.OldQuantity,
                        newQuantity = item.NewQuantity
                    }),
                    removedItems = fixResult.RemovedItems.Select(item => new
                    {
                        name = item.Name,
                        reason = item.Reason
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-fixing stock issues");
                return Json(new { success = false, message = ex.Message });
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

        /// <summary>
        /// Helper method to handle both AJAX and regular responses
        /// </summary>
        private IActionResult HandleResponse(bool success, string message)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success, message });
            }

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                if (message.Contains("session expired"))
                {
                    return RedirectToAction("Login", "Account");
                }
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction("ViewCart");
        }
    }
}