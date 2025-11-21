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
    public class ReviewController : ControllerBase<ReviewController>
    {
        private readonly IReviewService _reviewService;

        public ReviewController(
            IReviewService reviewService,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// GET: Review/Create/5
        /// Displays the review creation form for a specific order
        /// </summary>
        public IActionResult Create(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("User {UserId} creating review for order {OrderId}", userId, orderId);

                var reviewModel = _reviewService.GetReviewFormForOrder(orderId, userId);

                if (reviewModel == null)
                {
                    _logger.LogWarning("User {UserId} attempted to review order {OrderId} - not found or access denied", userId, orderId);
                    TempData["ErrorMessage"] = "Order not found or access denied.";
                    return RedirectToAction("MyOrders", "Order");
                }

                if (!reviewModel.CanReview)
                {
                    if (reviewModel.HasReview)
                    {
                        _logger.LogInformation("User {UserId} attempted to review order {OrderId} - already reviewed", userId, orderId);
                        TempData["InfoMessage"] = "You have already reviewed this order.";
                        return RedirectToAction("Details", "Order", new { id = orderId });
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} attempted to review order {OrderId} - not eligible", userId, orderId);
                        TempData["ErrorMessage"] = "This order is not eligible for review at this time.";
                        return RedirectToAction("MyOrders", "Order");
                    }
                }

                return View(reviewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading review form for order {OrderId}", orderId);
                TempData["ErrorMessage"] = "An error occurred while loading the review form.";
                return RedirectToAction("MyOrders", "Order");
            }
        }

        /// <summary>
        /// POST: Review/Create
        /// Processes the review creation form submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
                    return View(model);
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("User {UserId} submitting review for order {OrderId}", userId, model.OrderId);

                // Validate using service layer
                var validation = _reviewService.ValidateReviewSubmission(model, userId);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Review submission validation failed for user {UserId}, order {OrderId}: {Message}",
                        userId, model.OrderId, validation.Message);
                    TempData["ErrorMessage"] = validation.Message;
                    return View(model);
                }

                var success = _reviewService.SubmitReview(model, userId);

                if (success)
                {
                    _logger.LogInformation("Review submitted successfully for user {UserId}, order {OrderId}", userId, model.OrderId);
                    TempData["SuccessMessage"] = "Thank you for your review! Your feedback helps us improve.";
                    return RedirectToAction("Details", "Order", new { id = model.OrderId });
                }
                else
                {
                    _logger.LogWarning("Review submission failed for user {UserId}, order {OrderId}", userId, model.OrderId);
                    TempData["ErrorMessage"] = "Unable to submit your review. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting review for order {OrderId}", model.OrderId);
                TempData["ErrorMessage"] = "An error occurred while submitting your review.";
                return View(model);
            }
        }

        /// <summary>
        /// GET: Review/MyReviews
        /// Displays the customer's review history
        /// </summary>
        public IActionResult MyReviews()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("User {UserId} accessing their reviews", userId);

                var reviewsModel = _reviewService.GetUserReviews(userId);
                return View(reviewsModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user reviews for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "An error occurred while retrieving your reviews.";
                return RedirectToAction("MyOrders", "Order");
            }
        }

        /// <summary>
        /// GET: Review/GetReviewForEdit
        /// AJAX endpoint to get review data for editing
        /// </summary>
        [HttpGet]
        public IActionResult GetReviewForEdit(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                _logger.LogInformation("User {UserId} loading review for edit, order {OrderId}", userId, orderId);

                // Validate edit permission using service layer
                var editValidation = _reviewService.ValidateReviewEdit(orderId, userId);
                if (!editValidation.CanEdit)
                {
                    _logger.LogWarning("Review edit validation failed for user {UserId}, order {OrderId}: {Message}",
                        userId, orderId, editValidation.Message);
                    return Json(new { success = false, message = editValidation.Message });
                }

                var reviewModel = _reviewService.GetReviewForEdit(orderId, userId);

                if (reviewModel == null)
                {
                    return Json(new { success = false, message = "Review not found." });
                }

                return Json(new
                {
                    success = true,
                    review = new
                    {
                        orderId = reviewModel.OrderId,
                        rating = reviewModel.Rating,
                        comment = reviewModel.Comment ?? ""
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading review for edit. OrderId: {OrderId}", orderId);
                return Json(new { success = false, message = "An error occurred while loading the review." });
            }
        }

        /// <summary>
        /// POST: Review/UpdateReview
        /// AJAX endpoint to update an existing review
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateReview(int orderId, int rating, string comment)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                _logger.LogInformation("User {UserId} updating review for order {OrderId}", userId, orderId);

                // Validate using service layer
                var validation = _reviewService.ValidateReviewUpdate(orderId, userId, rating, comment);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Review update validation failed for user {UserId}, order {OrderId}: {Message}",
                        userId, orderId, validation.Message);
                    return Json(new { success = false, message = validation.Message });
                }

                var success = _reviewService.UpdateReview(orderId, userId, rating, comment);

                if (success)
                {
                    _logger.LogInformation("Review updated successfully for user {UserId}, order {OrderId}", userId, orderId);
                    return Json(new { success = true, message = "Review updated successfully!" });
                }
                else
                {
                    _logger.LogWarning("Review update failed for user {UserId}, order {OrderId}", userId, orderId);
                    return Json(new { success = false, message = "Unable to update your review. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating review for order {OrderId}", orderId);
                return Json(new { success = false, message = "An error occurred while updating your review." });
            }
        }

        /// <summary>
        /// GET: Review/AllReviews
        /// Displays all reviews (accessible to all authenticated users)
        /// </summary>
        public IActionResult AllReviews()
        {
            try
            {
                _logger.LogInformation("User accessing all reviews page");

                var reviewsModel = _reviewService.GetAllReviews();

                // Check if user is admin to show different view/features
                var userRole = HttpContext.Session.GetString("UserRole");
                ViewBag.IsAdmin = userRole == "Admin";

                return View(reviewsModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all reviews");
                TempData["ErrorMessage"] = "An error occurred while retrieving reviews.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Helper method to get current user ID from multiple sources
        /// </summary>
        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId")
                   ?? HttpContext.Session.GetString("UserName")
                   ?? User.Identity.Name;
        }
    }
}