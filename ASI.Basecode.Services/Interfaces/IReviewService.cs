using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IReviewService
    {
        ReviewViewModel GetReviewFormForOrder(int orderId, string userId);
        bool SubmitReview(ReviewViewModel model, string userId);
        MyReviewsViewModel GetUserReviews(string userId);
        MyReviewsViewModel GetAllReviews();
    }
}