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

        /// <summary>
        /// GET: Profile
        /// Displays the user's profile information
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in session for profile view");
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

        /// <summary>
        /// GET: Profile/Edit
        /// Displays the profile edit form
        /// </summary>
        public IActionResult Edit(string returnUrl = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in session for profile edit");
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("User {UserId} accessing profile edit", userId);

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

        /// <summary>
        /// POST: Profile/Edit
        /// Processes profile update form submission
        /// </summary>
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
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("User {UserId} updating profile", userId);

                model.UserId = userId;
                _profileService.UpdateProfile(model);

                // Update session name if changed
                HttpContext.Session.SetString("UserName", model.Name);

                _logger.LogInformation("Profile updated successfully for user {UserId}", userId);
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
                _logger.LogError(ex, "Error occurred while updating profile for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "An error occurred while updating your profile.";
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
        }

        /// <summary>
        /// Helper method to get current user ID from session
        /// </summary>
        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId");
        }
    }
}