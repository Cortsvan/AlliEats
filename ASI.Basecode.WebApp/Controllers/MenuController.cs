using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    }
}