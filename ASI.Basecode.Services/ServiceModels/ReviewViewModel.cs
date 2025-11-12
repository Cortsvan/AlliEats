using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class ReviewViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Order number is required")]
        public string OrderNumber { get; set; }

        [Required(ErrorMessage = "Please provide a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Comment { get; set; }

        public DateTime OrderDate { get; set; }
        public decimal OrderTotal { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public bool CanReview { get; set; }
        public bool HasReview { get; set; }
        public ReviewViewModel ExistingReview { get; set; }

        public ReviewViewModel()
        {
            OrderItems = new List<OrderItemViewModel>();
        }
    }

    public class MyReviewsViewModel
    {
        public List<UserReviewViewModel> Reviews { get; set; }

        public MyReviewsViewModel()
        {
            Reviews = new List<UserReviewViewModel>();
        }
    }

    public class UserReviewViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderTotal { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }

        public UserReviewViewModel()
        {
            OrderItems = new List<OrderItemViewModel>();
        }
    }
}