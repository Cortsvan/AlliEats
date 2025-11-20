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

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    public class ProfileController : ControllerBase<ProfileController>
    {
        private readonly IProfileService _profileService;

        public ProfileController(
            IProfileService profileService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _profileService = profileService;
        }

        // GET: Profile
        public IActionResult Index()
        {
            try
            {
                // ✅ IMPROVED: First try to get the actual UserId (email) from session
                var userId = HttpContext.Session.GetString("UserId");

                // If not found, log the issue and redirect to login
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in session. Session keys: {Keys}",
                        string.Join(", ", HttpContext.Session.Keys));
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("Getting profile for userId: {UserId}", userId);
                var profile = _profileService.GetProfile(userId);
                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving profile");
                TempData["ErrorMessage"] = "An error occurred while retrieving your profile.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Profile/Edit
        public IActionResult Edit(string returnUrl = null)
        {
            try
            {
                // ✅ IMPROVED: First try to get the actual UserId (email) from session
                var userId = HttpContext.Session.GetString("UserId");

                // If not found, log the issue and redirect to login
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in session. Session keys: {Keys}",
                        string.Join(", ", HttpContext.Session.Keys));
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                var profile = _profileService.GetProfile(userId);
                
                // Store return URL in ViewBag for the form to use
                ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
                
                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving profile for editing");
                TempData["ErrorMessage"] = "An error occurred while loading your profile.";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProfileViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            try
            {
                // ✅ IMPROVED: Get the actual UserId (email) from session
                var userId = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                model.UserId = userId;
                _profileService.UpdateProfile(model);

                // Update session name if changed
                HttpContext.Session.SetString("UserName", model.Name);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                
                // Redirect to return URL if provided, otherwise go to Profile Index
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating profile");
                TempData["ErrorMessage"] = "An error occurred while updating your profile.";
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
        }

    }
}