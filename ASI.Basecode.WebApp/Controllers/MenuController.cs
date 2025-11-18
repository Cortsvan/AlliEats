using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
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

        // GET: Menu/Browse
        public IActionResult Browse()
        {
            // Restrict admin access
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                TempData["ErrorMessage"] = "Admins cannot browse menu for ordering.";
                return RedirectToAction("Index", "Home");
            }

            var menuItems = _menuService.GetActiveMenuItems();
            return View(menuItems);
        }

        // GET: Menu/Search - API endpoint for header search
        [HttpGet]
        public IActionResult Search(string q, int limit = 5)
        {
            // Restrict admin access
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                return Json(new { success = false, message = "Access denied" });
            }

            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Json(new { success = true, items = new object[0] });
            }

            try
            {
                var allMenuItems = _menuService.GetActiveMenuItems();
                var searchResults = allMenuItems
                    .Where(item => item.Name.ToLower().Contains(q.ToLower()) ||
                                  (item.Description?.ToLower().Contains(q.ToLower()) ?? false) ||
                                  item.Category.ToLower().Contains(q.ToLower()))
                    .Take(limit)
                    .Select(item => new
                    {
                        id = item.Id,
                        name = item.Name,
                        description = item.Description ?? "",
                        price = item.Price,
                        category = item.Category,
                        imagePath = item.ImagePath ?? "/img/placeholder-food.png",
                        stock = item.Stock,
                        isAvailable = item.Stock > 0
                    })
                    .ToList();

                return Json(new { success = true, items = searchResults, total = searchResults.Count });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error during menu search");
                return Json(new { success = false, message = "Search failed" });
            }
        }
    }
}