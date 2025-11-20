using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IReviewService
    {
        ReviewViewModel GetReviewFormForOrder(int orderId, string userId);
        bool SubmitReview(ReviewViewModel model, string userId);
        MyReviewsViewModel GetUserReviews(string userId);
        MyReviewsViewModel GetAllReviews();
        ReviewViewModel GetReviewForEdit(int orderId, string userId);
        bool UpdateReview(int orderId, string userId, int rating, string comment);

        // Dashboard methods
        MyReviewsViewModel GetFeaturedReviews(int count = 6);
        double GetAverageRating();

        // New validation methods
        (bool IsValid, string Message) ValidateReviewSubmission(ReviewViewModel model, string userId);
        (bool IsValid, string Message) ValidateReviewUpdate(int orderId, string userId, int rating, string comment);
        (bool CanEdit, string Message) ValidateReviewEdit(int orderId, string userId);
    }
}