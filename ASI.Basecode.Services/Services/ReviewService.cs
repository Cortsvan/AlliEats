using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IOrderRepository _orderRepository;

        public ReviewService(IReviewRepository reviewRepository, IOrderRepository orderRepository)
        {
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
        }

        public ReviewViewModel GetReviewFormForOrder(int orderId, string userId)
        {
            var order = _orderRepository.GetOrderById(orderId);

            if (order == null || order.UserId != userId)
                return null;

            var existingReview = _reviewRepository.GetReviewByOrderId(orderId);
            var canReview = _reviewRepository.CanUserReviewOrder(orderId, userId);

            return new ReviewViewModel
            {
                OrderId = orderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.CreatedTime,
                OrderTotal = order.TotalAmount,
                CanReview = canReview,
                HasReview = existingReview != null,
                ExistingReview = existingReview != null ? new ReviewViewModel
                {
                    Rating = existingReview.Rating,
                    Comment = existingReview.Comment
                } : null,
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    MenuItemName = oi.MenuItemName,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.TotalPrice,
                    MenuItemImagePath = oi.MenuItem.ImagePath
                }).ToList()
            };
        }

        public ReviewViewModel GetReviewForEdit(int orderId, string userId)
        {
            var review = _reviewRepository.GetReviewByOrderId(orderId);

            if (review == null || review.UserId != userId)
                return null;

            return new ReviewViewModel
            {
                OrderId = orderId,
                Rating = review.Rating,
                Comment = review.Comment
            };
        }

        public bool SubmitReview(ReviewViewModel model, string userId)
        {
            try
            {
                if (!_reviewRepository.CanUserReviewOrder(model.OrderId, userId))
                    return false;

                var review = new Review
                {
                    OrderId = model.OrderId,
                    UserId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedTime = DateTime.Now,
                    CreatedBy = userId
                };

                _reviewRepository.AddReview(review);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateReview(int orderId, string userId, int rating, string comment)
        {
            try
            {
                var existingReview = _reviewRepository.GetReviewByOrderId(orderId);

                if (existingReview == null || existingReview.UserId != userId)
                    return false;

                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.UpdatedTime = DateTime.Now;
                existingReview.UpdatedBy = userId;

                _reviewRepository.UpdateReview(existingReview);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public MyReviewsViewModel GetUserReviews(string userId)
        {
            var reviews = _reviewRepository.GetReviewsByUserId(userId);

            return new MyReviewsViewModel
            {
                Reviews = reviews.Select(r => new UserReviewViewModel
                {
                    OrderId = r.OrderId,
                    OrderNumber = r.Order.OrderNumber,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.CreatedTime,
                    OrderDate = r.Order.CreatedTime,
                    OrderTotal = r.Order.TotalAmount,
                    OrderItems = r.Order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        MenuItemName = oi.MenuItemName,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                        TotalPrice = oi.TotalPrice,
                        MenuItemImagePath = oi.MenuItem?.ImagePath
                    }).ToList()
                }).ToList()
            };
        }

        public MyReviewsViewModel GetAllReviews()
        {
            var reviews = _reviewRepository.GetAllReviews();

            return new MyReviewsViewModel
            {
                Reviews = reviews.Select(r => new UserReviewViewModel
                {
                    OrderId = r.OrderId,
                    OrderNumber = r.Order.OrderNumber,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.CreatedTime,
                    OrderDate = r.Order.CreatedTime,
                    OrderTotal = r.Order.TotalAmount,
                    OrderItems = r.Order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        MenuItemName = oi.MenuItemName,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                        TotalPrice = oi.TotalPrice,
                        MenuItemImagePath = oi.MenuItem?.ImagePath
                    }).ToList()
                }).ToList()
            };
        }

        //dashboard methods
        public MyReviewsViewModel GetFeaturedReviews(int count = 6)
        {
            try
            {
                var allReviewsResult = GetAllReviews();
                var featuredReviews = allReviewsResult.Reviews
                    .Where(r => !string.IsNullOrEmpty(r.Comment) && r.Rating >= 4)
                    .OrderByDescending(r => r.Rating)
                    .ThenByDescending(r => r.ReviewDate)
                    .Take(count)
                    .ToList();

                return new MyReviewsViewModel
                {
                    Reviews = featuredReviews
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting featured reviews: {ex.Message}", ex);
            }
        }

        public double GetAverageRating()
        {
            try
            {
                var allReviewsResult = GetAllReviews();
                return allReviewsResult.Reviews.Any()
                    ? allReviewsResult.Reviews.Average(r => r.Rating)
                    : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating average rating: {ex.Message}", ex);
            }
        }


        // review validation methods
        public (bool IsValid, string Message) ValidateReviewSubmission(ReviewViewModel model, string userId)
        {
            try
            {
                if (model == null)
                {
                    return (false, "Invalid review data.");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return (false, "User authentication required.");
                }

                if (model.Rating < 1 || model.Rating > 5)
                {
                    return (false, "Rating must be between 1 and 5 stars.");
                }

                if (!string.IsNullOrEmpty(model.Comment) && model.Comment.Length > 1000)
                {
                    return (false, "Comment cannot exceed 1000 characters.");
                }

                if (!_reviewRepository.CanUserReviewOrder(model.OrderId, userId))
                {
                    var existingReview = _reviewRepository.GetReviewByOrderId(model.OrderId);
                    if (existingReview != null)
                    {
                        return (false, "You have already reviewed this order.");
                    }
                    return (false, "This order is not eligible for review at this time.");
                }

                return (true, "Review data is valid.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating review submission: {ex.Message}", ex);
            }
        }

        public (bool IsValid, string Message) ValidateReviewUpdate(int orderId, string userId, int rating, string comment)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return (false, "User authentication required.");
                }

                if (rating < 1 || rating > 5)
                {
                    return (false, "Rating must be between 1 and 5 stars.");
                }

                if (!string.IsNullOrEmpty(comment) && comment.Length > 1000)
                {
                    return (false, "Comment cannot exceed 1000 characters.");
                }

                var existingReview = _reviewRepository.GetReviewByOrderId(orderId);
                if (existingReview == null)
                {
                    return (false, "Review not found.");
                }

                if (existingReview.UserId != userId)
                {
                    return (false, "Access denied. This review does not belong to you.");
                }

                return (true, "Review update data is valid.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating review update: {ex.Message}", ex);
            }
        }

        public (bool CanEdit, string Message) ValidateReviewEdit(int orderId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return (false, "User authentication required.");
                }

                var review = _reviewRepository.GetReviewByOrderId(orderId);
                if (review == null)
                {
                    return (false, "Review not found.");
                }

                if (review.UserId != userId)
                {
                    return (false, "Access denied. This review does not belong to you.");
                }

                return (true, "Review can be edited.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating review edit: {ex.Message}", ex);
            }
        }
    }
}