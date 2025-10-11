using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using ASI.Basecode.WebApp.Attributes;
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
    [AdminOnly]
    public class AdminMenuController : ControllerBase<AdminMenuController>
    {
        private readonly IMenuService _menuService;

        public AdminMenuController(
            IMenuService menuService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _menuService = menuService;
        }

        // GET: AdminMenu/ViewItems
        public IActionResult ViewItems()
        {
            var menuItems = _menuService.GetAllMenuItems();
            return View(menuItems);
        }

        // GET: AdminMenu/AddItem
        public IActionResult AddItem()
        {
            return View(new MenuItemViewModel());
        }

        // POST: AdminMenu/AddItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddItem(MenuItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _menuService.AddMenuItem(model);
                TempData["SuccessMessage"] = "Menu item added successfully!";
                return RedirectToAction("ViewItems");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while adding the menu item.";
                return View(model);
            }
        }

        // GET: AdminMenu/EditItem/5
        public IActionResult EditItem(int id)
        {
            var menuItem = _menuService.GetMenuItemById(id);
            if (menuItem == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction("ViewItems");
            }

            return View(menuItem);
        }

        // POST: AdminMenu/EditItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditItem(int id, MenuItemViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _menuService.UpdateMenuItem(model);
                TempData["SuccessMessage"] = "Menu item updated successfully!";
                return RedirectToAction("ViewItems");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the menu item.";
                return View(model);
            }
        }

        // POST: AdminMenu/DeleteItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteItem(int id)
        {
            try
            {
                _menuService.DeleteMenuItem(id);
                TempData["SuccessMessage"] = "Menu item deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the menu item.";
            }

            return RedirectToAction("ViewItems");
        }
    }
}