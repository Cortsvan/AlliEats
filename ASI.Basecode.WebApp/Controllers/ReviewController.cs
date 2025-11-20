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

        // GET: Review/Create/5
        public IActionResult Create(int orderId)
        {
            try
            {
                // Restrict admin access
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole == "Admin")
                {
                    TempData["ErrorMessage"] = "Admins cannot create reviews.";
                    return RedirectToAction("Index", "Home");
                }

                // Get userId from session
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                var reviewModel = _reviewService.GetReviewFormForOrder(orderId, userId);

                if (reviewModel == null)
                {
                    TempData["ErrorMessage"] = "Order not found or access denied.";
                    return RedirectToAction("MyOrders", "Order");
                }

                if (!reviewModel.CanReview)
                {
                    if (reviewModel.HasReview)
                    {
                        TempData["InfoMessage"] = "You have already reviewed this order.";
                        return RedirectToAction("Details", "Order", new { id = orderId });
                    }
                    else
                    {
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

        // POST: Review/Create
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

                // Get userId from session
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                var success = _reviewService.SubmitReview(model, userId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Thank you for your review! Your feedback helps us improve.";
                    return RedirectToAction("Details", "Order", new { id = model.OrderId });
                }
                else
                {
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

        // GET: Review/MyReviews
        public IActionResult MyReviews()
        {
            try
            {
                // Restrict admin access
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole == "Admin")
                {
                    TempData["ErrorMessage"] = "Admins cannot access reviews.";
                    return RedirectToAction("Index", "Home");
                }

                // Get userId from session
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                var reviewsModel = _reviewService.GetUserReviews(userId);
                return View(reviewsModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user reviews");
                TempData["ErrorMessage"] = "An error occurred while retrieving your reviews.";
                return RedirectToAction("MyOrders", "Order");
            }
        }

        // GET: Review/GetReviewForEdit
        [HttpGet]
        public IActionResult GetReviewForEdit(int orderId)
        {
            try
            {
                // Restrict admin access
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole == "Admin")
                {
                    return Json(new { success = false, message = "Admins cannot edit reviews." });
                }

                // Get userId from session
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                var reviewModel = _reviewService.GetReviewForEdit(orderId, userId);

                if (reviewModel == null)
                {
                    return Json(new { success = false, message = "Review not found or access denied." });
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

        // POST: Review/UpdateReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateReview(int orderId, int rating, string comment)
        {
            try
            {
                // Get userId from session
                var userId = HttpContext.Session.GetString("UserId")
                            ?? HttpContext.Session.GetString("UserName")
                            ?? User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User session expired. Please login again." });
                }

                // Validate rating
                if (rating < 1 || rating > 5)
                {
                    return Json(new { success = false, message = "Rating must be between 1 and 5 stars." });
                }

                // Validate comment length
                if (!string.IsNullOrEmpty(comment) && comment.Length > 1000)
                {
                    return Json(new { success = false, message = "Comment cannot exceed 1000 characters." });
                }

                var success = _reviewService.UpdateReview(orderId, userId, rating, comment);

                if (success)
                {
                    return Json(new { success = true, message = "Review updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to update your review. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating review for order {OrderId}", orderId);
                return Json(new { success = false, message = "An error occurred while updating your review." });
            }
        }

        // GET: Review/AllReviews - Accessible to all authenticated users
        public IActionResult AllReviews()
        {
            try
            {
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
    }
}