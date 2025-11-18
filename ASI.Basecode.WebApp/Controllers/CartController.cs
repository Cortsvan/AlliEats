using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Services;
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

        // GET: Cart/ViewCart
        public IActionResult ViewCart()
        {
            // Restrict admin access
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                TempData["ErrorMessage"] = "Admins cannot access cart functionality.";
                return RedirectToAction("Index", "Home");
            }

            // Get userId from session - try multiple session keys
            var userId = HttpContext.Session.GetString("UserId")
                        ?? HttpContext.Session.GetString("UserName")
                        ?? User.Identity.Name;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            var cart = _cartService.GetCartByUserId(userId);
            return View(cart);
        }

        // POST: Cart/AddItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddItem(int menuItemId, int quantity = 1)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                return Json(new { success = false, message = "Admins cannot add items to cart." });
            }

            try
            {
                // Get userId from session - try multiple session keys
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                _cartService.AddToCart(userId, menuItemId, quantity);

                var itemCount = _cartService.GetCartItemCount(userId);
                return Json(new { success = true, message = "Item added to cart!", itemCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Cart/UpdateItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateItem(int cartItemId, int quantity)
        {
            try
            {
                _cartService.UpdateCartItem(cartItemId, quantity);
                TempData["SuccessMessage"] = "Cart updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("ViewCart");
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int cartItemId)
        {
            try
            {
                _cartService.RemoveFromCart(cartItemId);

                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Item removed from cart!" });
                }

                TempData["SuccessMessage"] = "Item removed from cart!";
            }
            catch (Exception ex)
            {
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("ViewCart");
        }

        // POST: Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            try
            {
                // Get userId from session - try multiple session keys
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    // Check if this is an AJAX request
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "User session expired. Please login again." });
                    }

                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _cartService.ClearCart(userId);

                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Cart cleared successfully!" });
                }

                TempData["SuccessMessage"] = "Cart cleared successfully!";
            }
            catch (Exception ex)
            {
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("ViewCart");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckStockAvailability()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                var cart = _cartService.GetCartByUserId(userId);
                if (cart == null || !cart.CartItems.Any())
                {
                    return Json(new { success = true, message = "Cart is empty.", stockIssues = new object[0] });
                }

                var stockIssues = new List<object>();
                var hasIssues = false;

                foreach (var cartItem in cart.CartItems)
                {
                    var menuItem = _menuService.GetMenuItemById(cartItem.MenuItemId);
                    if (menuItem == null || !menuItem.IsActive)
                    {
                        stockIssues.Add(new
                        {
                            cartItemId = cartItem.Id,
                            menuItemId = cartItem.MenuItemId,
                            itemName = cartItem.MenuItemName,
                            requestedQuantity = cartItem.Quantity,
                            availableStock = 0,
                            issue = "unavailable",
                            message = $"{cartItem.MenuItemName} is no longer available"
                        });
                        hasIssues = true;
                    }
                    else if (menuItem.Stock < cartItem.Quantity)
                    {
                        stockIssues.Add(new
                        {
                            cartItemId = cartItem.Id,
                            menuItemId = cartItem.MenuItemId,
                            itemName = cartItem.MenuItemName,
                            requestedQuantity = cartItem.Quantity,
                            availableStock = menuItem.Stock,
                            issue = menuItem.Stock == 0 ? "out-of-stock" : "insufficient",
                            message = menuItem.Stock == 0
                                ? $"{cartItem.MenuItemName} is now out of stock"
                                : $"Only {menuItem.Stock} {cartItem.MenuItemName} available (you have {cartItem.Quantity} in cart)"
                        });
                        hasIssues = true;
                    }
                }

                return Json(new
                {
                    success = true,
                    hasIssues = hasIssues,
                    stockIssues = stockIssues,
                    message = hasIssues ? "Stock issues detected" : "All items are available"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // NEW: Auto-fix stock issues
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AutoFixStockIssues()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                var cart = _cartService.GetCartByUserId(userId);
                if (cart == null || !cart.CartItems.Any())
                {
                    return Json(new { success = true, message = "Cart is empty." });
                }

                var fixedItems = new List<object>();
                var removedItems = new List<object>();

                foreach (var cartItem in cart.CartItems)
                {
                    var menuItem = _menuService.GetMenuItemById(cartItem.MenuItemId);

                    if (menuItem == null || !menuItem.IsActive || menuItem.Stock == 0)
                    {
                        // Remove unavailable items
                        _cartService.RemoveFromCart(cartItem.Id);
                        removedItems.Add(new
                        {
                            name = cartItem.MenuItemName,
                            reason = "no longer available"
                        });
                    }
                    else if (menuItem.Stock < cartItem.Quantity)
                    {
                        // Adjust quantity to available stock
                        _cartService.UpdateCartItem(cartItem.Id, menuItem.Stock);
                        fixedItems.Add(new
                        {
                            name = cartItem.MenuItemName,
                            oldQuantity = cartItem.Quantity,
                            newQuantity = menuItem.Stock
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Stock issues have been resolved",
                    fixedItems = fixedItems,
                    removedItems = removedItems
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}