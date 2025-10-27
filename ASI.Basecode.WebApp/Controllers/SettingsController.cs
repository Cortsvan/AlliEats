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
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    public class SettingsController : ControllerBase<SettingsController>
    {
        private readonly IUserService _userService;

        public SettingsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUserService userService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var currentUser = _session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUser))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ChangePasswordViewModel
            {
                Email = currentUser
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = _session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUser) || currentUser != model.Email)
            {
                TempData["ErrorMessage"] = "Invalid session. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var isChanged = _userService.ChangePassword(model.Email, model.CurrentPassword, model.NewPassword);

                if (isChanged)
                {
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Current password is incorrect. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while changing your password. Please try again.";
                return View(model);
            }
        }
    }
}