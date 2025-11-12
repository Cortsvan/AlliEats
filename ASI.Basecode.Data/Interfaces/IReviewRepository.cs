using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IReviewRepository
    {
        Review GetReviewByOrderId(int orderId);
        IEnumerable<Review> GetReviewsByUserId(string userId);
        IEnumerable<Review> GetAllReviews();
        void AddReview(Review review);
        void UpdateReview(Review review);
        void DeleteReview(int id);
        bool ReviewExists(int orderId);
        bool CanUserReviewOrder(int orderId, string userId);
    }
}