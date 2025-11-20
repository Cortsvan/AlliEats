using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class PaymentCardRepository : BaseRepository, IPaymentCardRepository
    {
        public PaymentCardRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<PaymentCard> GetCardsByUserId(string userId)
        {
            return this.GetDbSet<PaymentCard>()
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IsDefault)
                .ThenByDescending(c => c.CreatedTime)
                .ToList();
        }

        public PaymentCard GetCardById(int id)
        {
            return this.GetDbSet<PaymentCard>().FirstOrDefault(c => c.Id == id);
        }

        public void AddCard(PaymentCard card)
        {
            this.GetDbSet<PaymentCard>().Add(card);
            UnitOfWork.SaveChanges();
        }

        public void UpdateCard(PaymentCard card)
        {
            this.GetDbSet<PaymentCard>().Update(card);
            UnitOfWork.SaveChanges();
        }

        public void DeleteCard(int id)
        {
            var card = GetCardById(id);
            if (card != null)
            {
                this.GetDbSet<PaymentCard>().Remove(card);
                UnitOfWork.SaveChanges();
            }
        }

        public bool CardExists(int id)
        {
            return this.GetDbSet<PaymentCard>().Any(c => c.Id == id);
        }

        public PaymentCard GetDefaultCard(string userId)
        {
            return this.GetDbSet<PaymentCard>()
                .FirstOrDefault(c => c.UserId == userId && c.IsDefault);
        }

        public void SetDefaultCard(string userId, int cardId)
        {
            var cards = this.GetDbSet<PaymentCard>().Where(c => c.UserId == userId).ToList();
            
            foreach (var card in cards)
            {
                card.IsDefault = (card.Id == cardId);
                card.UpdatedTime = DateTime.Now;
                card.UpdatedBy = userId;
            }

            UnitOfWork.SaveChanges();
        }
    }
}
