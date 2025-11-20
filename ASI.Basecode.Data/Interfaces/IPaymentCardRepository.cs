using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IPaymentCardRepository
    {
        IEnumerable<PaymentCard> GetCardsByUserId(string userId);
        PaymentCard GetCardById(int id);
        void AddCard(PaymentCard card);
        void UpdateCard(PaymentCard card);
        void DeleteCard(int id);
        bool CardExists(int id);
        PaymentCard GetDefaultCard(string userId);
        void SetDefaultCard(string userId, int cardId);
    }
}
