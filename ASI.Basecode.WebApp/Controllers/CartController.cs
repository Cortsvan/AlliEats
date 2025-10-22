using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
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
    public class CartController : ControllerBase<CartController>
    {
        private readonly ICartService _cartService;

        public CartController(
            ICartService cartService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _cartService = cartService;
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
    }
}