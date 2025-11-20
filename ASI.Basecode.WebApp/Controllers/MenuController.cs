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
    [CustomerOnly] // Custom attribute to restrict admin access
    public class MenuController : ControllerBase<MenuController>
    {
        private readonly IMenuService _menuService;

        public MenuController(
            IMenuService menuService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _menuService = menuService;
        }

        /// <summary>
        /// GET: Menu/Browse
        /// Displays the menu browsing page for customers
        /// </summary>
        public IActionResult Browse()
        {
            try
            {
                _logger.LogInformation("Customer accessing menu browse page");

                var menuItems = _menuService.GetActiveMenuItems();

                _logger.LogInformation("Retrieved {ItemCount} active menu items for browsing", menuItems?.Count() ?? 0);

                return View(menuItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading menu browse page");
                TempData["ErrorMessage"] = "An error occurred while loading the menu. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// GET: Menu/Search
        /// API endpoint for header search functionality
        /// </summary>
        [HttpGet]
        public IActionResult Search(string q, int limit = 5)
        {
            try
            {
                // Validate input parameters
                if (limit <= 0 || limit > 20)
                {
                    limit = 5; // Default to safe limit
                }

                _logger.LogInformation("Menu search requested: query='{Query}', limit={Limit}", q, limit);

                // Use service layer for search logic
                var searchResult = _menuService.SearchMenuItems(q, limit);

                if (!searchResult.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = searchResult.Message,
                        items = new object[0],
                        total = 0
                    });
                }

                // Transform to anonymous objects for JSON response
                var responseItems = searchResult.Items.Select(item => new
                {
                    id = item.Id,
                    name = item.Name,
                    description = item.Description,
                    price = item.Price,
                    category = item.Category,
                    imagePath = item.ImagePath,
                    stock = item.Stock,
                    isAvailable = item.IsAvailable
                });

                _logger.LogInformation("Menu search completed: found {ResultCount} items", searchResult.TotalResults);

                return Json(new
                {
                    success = true,
                    items = responseItems,
                    total = searchResult.TotalResults,
                    message = searchResult.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during menu search for query '{Query}'", q);
                return Json(new
                {
                    success = false,
                    message = "Search failed. Please try again.",
                    items = new object[0],
                    total = 0
                });
            }
        }

        /// <summary>
        /// GET: Menu/Details/{id}
        /// Shows detailed information about a specific menu item
        /// </summary>
        public IActionResult Details(int id)
        {
            try
            {
                _logger.LogInformation("Customer requesting menu item details for item {MenuItemId}", id);

                if (!_menuService.MenuItemExists(id))
                {
                    _logger.LogWarning("Customer attempted to access non-existent menu item {MenuItemId}", id);
                    TempData["ErrorMessage"] = "Menu item not found.";
                    return RedirectToAction("Browse");
                }

                var menuItem = _menuService.GetMenuItemById(id);
                if (menuItem == null || !menuItem.IsActive)
                {
                    _logger.LogWarning("Customer attempted to access inactive menu item {MenuItemId}", id);
                    TempData["ErrorMessage"] = "This menu item is not available.";
                    return RedirectToAction("Browse");
                }

                return View(menuItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving menu item details for item {MenuItemId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading item details.";
                return RedirectToAction("Browse");
            }
        }

        /// <summary>
        /// GET: Menu/Categories
        /// API endpoint to get available categories
        /// </summary>
        [HttpGet]
        public IActionResult Categories()
        {
            try
            {
                _logger.LogInformation("Fetching menu categories");

                var categories = _menuService.GetTopCategories(10); // Get more categories for full list

                return Json(new
                {
                    success = true,
                    categories = categories,
                    total = categories.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving menu categories");
                return Json(new
                {
                    success = false,
                    message = "Failed to load categories",
                    categories = new string[0],
                    total = 0
                });
            }
        }

        /// <summary>
        /// GET: Menu/ValidateAvailability/{id}
        /// AJAX endpoint to check if a menu item is available for ordering
        /// </summary>
        [HttpGet]
        public IActionResult ValidateAvailability(int id, int quantity = 1)
        {
            try
            {
                _logger.LogInformation("Validating availability for menu item {MenuItemId}, quantity {Quantity}", id, quantity);

                if (!_menuService.MenuItemExists(id))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Menu item not found.",
                        available = false
                    });
                }

                var menuItem = _menuService.GetMenuItemById(id);
                if (menuItem == null || !menuItem.IsActive)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This item is not available.",
                        available = false
                    });
                }

                var hasStock = _menuService.HasSufficientStock(id, quantity);
                var message = hasStock
                    ? "Item is available"
                    : $"Only {menuItem.Stock} items available";

                return Json(new
                {
                    success = true,
                    available = hasStock,
                    message = message,
                    stock = menuItem.Stock
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating availability for menu item {MenuItemId}", id);
                return Json(new
                {
                    success = false,
                    message = "Error checking availability",
                    available = false
                });
            }
        }
    }
}