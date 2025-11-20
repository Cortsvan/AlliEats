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
        private readonly IPaymentCardService _paymentCardService;

        public SettingsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUserService userService,
            IPaymentCardService paymentCardService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
            _paymentCardService = paymentCardService;
        }

        /// <summary>
        /// GET: Settings
        /// Main settings page (accessible to all authenticated users)
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET: Settings/ChangePassword
        /// Displays password change form (accessible to all authenticated users)
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            try
            {
                var currentUser = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUser))
                {
                    _logger.LogWarning("User session expired when accessing change password");
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("User {UserId} accessing change password page", currentUser);

                var model = new ChangePasswordViewModel
                {
                    Email = currentUser
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading change password page");
                TempData["ErrorMessage"] = "An error occurred while loading the page.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST: Settings/ChangePassword
        /// Processes password change request (accessible to all authenticated users)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var currentUser = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUser))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                // Validate session consistency
                if (!_userService.ValidateUserSession(currentUser, model.Email))
                {
                    _logger.LogWarning("Session validation failed for user {SessionUser} vs {ModelUser}", currentUser, model.Email);
                    TempData["ErrorMessage"] = "Invalid session. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("User {UserId} attempting password change", currentUser);

                // Validate using service layer
                var validation = _userService.ValidatePasswordChange(model.Email, model.CurrentPassword, model.NewPassword);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Password change validation failed for user {UserId}: {Message}", currentUser, validation.Message);
                    TempData["ErrorMessage"] = validation.Message;
                    return View(model);
                }

                var isChanged = _userService.ChangePassword(model.Email, model.CurrentPassword, model.NewPassword);

                if (isChanged)
                {
                    _logger.LogInformation("Password changed successfully for user {UserId}", currentUser);
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("Password change failed for user {UserId}", currentUser);
                    TempData["ErrorMessage"] = "Failed to change password. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user {UserId}", model?.Email);
                TempData["ErrorMessage"] = "An error occurred while changing your password. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// GET: Settings/PaymentMethods
        /// Displays payment methods (customer-only functionality)
        /// </summary>
        [HttpGet]
        public IActionResult PaymentMethods()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if admin is trying to access payment methods
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole == "Admin")
                {
                    TempData["ErrorMessage"] = "Admins cannot access payment methods.";
                    return RedirectToAction("Index");
                }

                _logger.LogInformation("User {UserId} accessing payment methods", userId);

                var cards = _paymentCardService.GetCardsByUserId(userId);
                return View(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment methods for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "An error occurred while retrieving your payment methods.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// GET: Settings/AddPaymentCard
        /// Displays add payment card form (customer-only functionality)
        /// </summary>
        [HttpGet]
        public IActionResult AddPaymentCard(string returnUrl = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if admin is trying to access payment methods
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole == "Admin")
                {
                    TempData["ErrorMessage"] = "Admins cannot manage payment methods.";
                    return RedirectToAction("Index");
                }

                _logger.LogInformation("User {UserId} accessing add payment card form", userId);

                ViewBag.ReturnUrl = returnUrl;
                return View(new PaymentCardViewModel { UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading add payment card page");
                TempData["ErrorMessage"] = "An error occurred while loading the page.";
                return RedirectToAction("PaymentMethods");
            }
        }

        /// <summary>
        /// POST: Settings/AddPaymentCard
        /// Processes add payment card form (customer-only functionality)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPaymentCard(PaymentCardViewModel model, string returnUrl = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                model.UserId = userId;

                if (!ModelState.IsValid)
                {
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                _logger.LogInformation("User {UserId} adding new payment card", userId);

                // Validate using service layer
                var validation = _paymentCardService.ValidateCardData(model);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Payment card validation failed for user {UserId}: {Message}", userId, validation.Message);
                    TempData["ErrorMessage"] = validation.Message;
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                _paymentCardService.AddCard(model);

                _logger.LogInformation("Payment card added successfully for user {UserId}", userId);
                TempData["SuccessMessage"] = "Payment card added successfully!";

                // Handle return URL for checkout flow
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("PaymentMethods");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment card for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "An error occurred while adding your payment card.";
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
        }

        /// <summary>
        /// GET: Settings/EditPaymentCard/5
        /// Displays edit payment card form (customer-only functionality)
        /// </summary>
        [HttpGet]
        public IActionResult EditPaymentCard(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("User {UserId} editing payment card {CardId}", userId, id);

                // Validate ownership using service layer
                if (!_paymentCardService.ValidateCardOwnership(id, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to edit card {CardId} they don't own", userId, id);
                    TempData["ErrorMessage"] = "Payment card not found or access denied.";
                    return RedirectToAction("PaymentMethods");
                }

                var card = _paymentCardService.GetCardById(id);
                return View(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment card {CardId} for edit", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the payment card.";
                return RedirectToAction("PaymentMethods");
            }
        }

        /// <summary>
        /// POST: Settings/EditPaymentCard/5
        /// Processes edit payment card form (customer-only functionality)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPaymentCard(PaymentCardViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                model.UserId = userId;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                _logger.LogInformation("User {UserId} updating payment card {CardId}", userId, model.Id);

                // Validate ownership using service layer
                if (!_paymentCardService.ValidateCardOwnership(model.Id, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to update card {CardId} they don't own", userId, model.Id);
                    TempData["ErrorMessage"] = "Access denied.";
                    return RedirectToAction("PaymentMethods");
                }

                _paymentCardService.UpdateCard(model);

                _logger.LogInformation("Payment card {CardId} updated successfully by user {UserId}", model.Id, userId);
                TempData["SuccessMessage"] = "Payment card updated successfully!";
                return RedirectToAction("PaymentMethods");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment card {CardId}", model.Id);
                TempData["ErrorMessage"] = "An error occurred while updating your payment card.";
                return View(model);
            }
        }

        /// <summary>
        /// POST: Settings/DeletePaymentCard/5
        /// AJAX endpoint to delete a payment card (customer-only functionality)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePaymentCard(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired." });
                }

                _logger.LogInformation("User {UserId} deleting payment card {CardId}", userId, id);

                // Validate ownership using service layer
                if (!_paymentCardService.ValidateCardOwnership(id, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to delete card {CardId} they don't own", userId, id);
                    return Json(new { success = false, message = "Access denied." });
                }

                _paymentCardService.DeleteCard(id, userId);

                _logger.LogInformation("Payment card {CardId} deleted successfully by user {UserId}", id, userId);
                return Json(new { success = true, message = "Payment card deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment card {CardId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the payment card." });
            }
        }

        /// <summary>
        /// POST: Settings/SetDefaultCard/5
        /// AJAX endpoint to set default payment card (customer-only functionality)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetDefaultCard(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired." });
                }

                _logger.LogInformation("User {UserId} setting default payment card {CardId}", userId, id);

                // Validate ownership using service layer
                if (!_paymentCardService.ValidateCardOwnership(id, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to set default card {CardId} they don't own", userId, id);
                    return Json(new { success = false, message = "Access denied." });
                }

                _paymentCardService.SetDefaultCard(userId, id);

                _logger.LogInformation("Default payment card set to {CardId} by user {UserId}", id, userId);
                return Json(new { success = true, message = "Default card updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default card {CardId}", id);
                return Json(new { success = false, message = "An error occurred while setting the default card." });
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