using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using ASI.Basecode.WebApp.Attributes;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    [AdminOnly]
    public class AdminMenuController : ControllerBase<AdminMenuController>
    {
        private readonly IMenuService _menuService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminMenuController(
            IMenuService menuService,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _menuService = menuService;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> AddItem(MenuItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Handle file upload
                if (model.ImageFile != null)
                {
                    model.ImagePath = await SaveImageAsync(model.ImageFile);
                }

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

        // POST: AdminMenu/EditItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(int id, MenuItemViewModel model)
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
                // Handle file upload for edit
                if (model.ImageFile != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(model.ImagePath))
                    {
                        DeleteImage(model.ImagePath);
                    }
                    model.ImagePath = await SaveImageAsync(model.ImageFile);
                }

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

        // POST: AdminMenu/DeleteItem/5
        [HttpPost("AdminMenu/DeleteItem/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteItem(int id)
        {
            try
            {
                var menuItem = _menuService.GetMenuItemById(id);
                if (menuItem != null && !string.IsNullOrEmpty(menuItem.ImagePath))
                {
                    DeleteImage(menuItem.ImagePath);
                }

                _menuService.DeleteMenuItem(id);
                TempData["SuccessMessage"] = "Menu item deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the menu item.";
            }

            return RedirectToAction("ViewItems");
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Create uploads directory if it doesn't exist
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "menu");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Return relative path for storage
            return "/uploads/menu/" + uniqueFileName;
        }

        private void DeleteImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }
    }
}