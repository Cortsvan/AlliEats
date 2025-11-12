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
    }
}