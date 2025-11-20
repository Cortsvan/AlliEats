using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IPaymentCardService
    {
        IEnumerable<PaymentCardViewModel> GetCardsByUserId(string userId);
        PaymentCardViewModel GetCardById(int id);
        void AddCard(PaymentCardViewModel cardModel);
        void UpdateCard(PaymentCardViewModel cardModel);
        void DeleteCard(int id, string userId);
        bool CardExists(int id);
        PaymentCardViewModel GetDefaultCard(string userId);
        void SetDefaultCard(string userId, int cardId);

        // validation methods
        bool ValidateCardOwnership(int cardId, string userId);
        (bool IsValid, string Message) ValidateCardData(PaymentCardViewModel model);
    }
}