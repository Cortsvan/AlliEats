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

        // GET: Settings/PaymentMethods
        [HttpGet]
        public IActionResult PaymentMethods()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var cards = _paymentCardService.GetCardsByUserId(userId);
                return View(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment methods for user {UserId}", userId);
                TempData["ErrorMessage"] = "An error occurred while retrieving your payment methods.";
                return RedirectToAction("Index");
            }
        }

        // GET: Settings/AddPaymentCard
        [HttpGet]
        public IActionResult AddPaymentCard()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new PaymentCardViewModel { UserId = userId });
        }

        // POST: Settings/AddPaymentCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPaymentCard(PaymentCardViewModel model, string returnUrl)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _paymentCardService.AddCard(model);
                TempData["SuccessMessage"] = "Payment card added successfully!";
                
                // If there's a return URL (coming from checkout), redirect there
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction("PaymentMethods");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment card for user {UserId}", userId);
                TempData["ErrorMessage"] = "An error occurred while adding your payment card.";
                return View(model);
            }
        }

        // GET: Settings/EditPaymentCard/5
        [HttpGet]
        public IActionResult EditPaymentCard(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var card = _paymentCardService.GetCardById(id);
                if (card == null || card.UserId != userId)
                {
                    TempData["ErrorMessage"] = "Payment card not found.";
                    return RedirectToAction("PaymentMethods");
                }

                return View(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment card {CardId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the payment card.";
                return RedirectToAction("PaymentMethods");
            }
        }

        // POST: Settings/EditPaymentCard/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPaymentCard(PaymentCardViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _paymentCardService.UpdateCard(model);
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

        // POST: Settings/DeletePaymentCard/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePaymentCard(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User session expired." });
            }

            try
            {
                _paymentCardService.DeleteCard(id, userId);
                return Json(new { success = true, message = "Payment card deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment card {CardId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the payment card." });
            }
        }

        // POST: Settings/SetDefaultCard/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetDefaultCard(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User session expired." });
            }

            try
            {
                _paymentCardService.SetDefaultCard(userId, id);
                return Json(new { success = true, message = "Default card updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default card {CardId}", id);
                return Json(new { success = false, message = "An error occurred while setting the default card." });
            }
        }
    }
}