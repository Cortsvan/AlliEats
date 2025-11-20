using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class PaymentCardService : IPaymentCardService
    {
        private readonly IPaymentCardRepository _paymentCardRepository;
        private readonly IMapper _mapper;

        public PaymentCardService(IPaymentCardRepository paymentCardRepository, IMapper mapper)
        {
            _paymentCardRepository = paymentCardRepository;
            _mapper = mapper;
        }

        public IEnumerable<PaymentCardViewModel> GetCardsByUserId(string userId)
        {
            var cards = _paymentCardRepository.GetCardsByUserId(userId);
            return _mapper.Map<IEnumerable<PaymentCardViewModel>>(cards);
        }

        public PaymentCardViewModel GetCardById(int id)
        {
            var card = _paymentCardRepository.GetCardById(id);
            return _mapper.Map<PaymentCardViewModel>(card);
        }

        public void AddCard(PaymentCardViewModel cardModel)
        {
            var card = _mapper.Map<PaymentCard>(cardModel);
            card.CreatedTime = DateTime.Now;
            card.CreatedBy = cardModel.UserId;
            card.UpdatedTime = DateTime.Now;
            card.UpdatedBy = cardModel.UserId;

            // Determine card type from card number
            if (string.IsNullOrEmpty(card.CardType))
            {
                card.CardType = DetermineCardType(card.CardNumber);
            }

            // If this is the first card or set as default, make it default
            var existingCards = _paymentCardRepository.GetCardsByUserId(cardModel.UserId);
            if (!existingCards.Any() || cardModel.IsDefault)
            {
                card.IsDefault = true;
                if (cardModel.IsDefault && existingCards.Any())
                {
                    // Remove default from other cards
                    foreach (var existingCard in existingCards)
                    {
                        existingCard.IsDefault = false;
                        existingCard.UpdatedTime = DateTime.Now;
                        existingCard.UpdatedBy = cardModel.UserId;
                        _paymentCardRepository.UpdateCard(existingCard);
                    }
                }
            }

            _paymentCardRepository.AddCard(card);
        }

        public void UpdateCard(PaymentCardViewModel cardModel)
        {
            var card = _paymentCardRepository.GetCardById(cardModel.Id);
            if (card == null)
                throw new InvalidOperationException("Card not found.");

            if (card.UserId != cardModel.UserId)
                throw new InvalidOperationException("Unauthorized access to card.");

            card.CardholderName = cardModel.CardholderName;
            card.CardNumber = cardModel.CardNumber;
            card.ExpiryDate = cardModel.ExpiryDate;
            card.CVV = cardModel.CVV;
            card.CardType = string.IsNullOrEmpty(cardModel.CardType) 
                ? DetermineCardType(cardModel.CardNumber) 
                : cardModel.CardType;
            card.UpdatedTime = DateTime.Now;
            card.UpdatedBy = cardModel.UserId;

            if (cardModel.IsDefault && !card.IsDefault)
            {
                SetDefaultCard(cardModel.UserId, cardModel.Id);
            }
            else
            {
                _paymentCardRepository.UpdateCard(card);
            }
        }

        public void DeleteCard(int id, string userId)
        {
            var card = _paymentCardRepository.GetCardById(id);
            if (card == null)
                throw new InvalidOperationException("Card not found.");

            if (card.UserId != userId)
                throw new InvalidOperationException("Unauthorized access to card.");

            bool wasDefault = card.IsDefault;
            _paymentCardRepository.DeleteCard(id);

            // If deleted card was default, set another card as default
            if (wasDefault)
            {
                var remainingCards = _paymentCardRepository.GetCardsByUserId(userId);
                var firstCard = remainingCards.FirstOrDefault();
                if (firstCard != null)
                {
                    firstCard.IsDefault = true;
                    firstCard.UpdatedTime = DateTime.Now;
                    firstCard.UpdatedBy = userId;
                    _paymentCardRepository.UpdateCard(firstCard);
                }
            }
        }

        public bool CardExists(int id)
        {
            return _paymentCardRepository.CardExists(id);
        }

        public PaymentCardViewModel GetDefaultCard(string userId)
        {
            var card = _paymentCardRepository.GetDefaultCard(userId);
            return _mapper.Map<PaymentCardViewModel>(card);
        }

        public void SetDefaultCard(string userId, int cardId)
        {
            var card = _paymentCardRepository.GetCardById(cardId);
            if (card == null)
                throw new InvalidOperationException("Card not found.");

            if (card.UserId != userId)
                throw new InvalidOperationException("Unauthorized access to card.");

            _paymentCardRepository.SetDefaultCard(userId, cardId);
        }

        private string DetermineCardType(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return "Unknown";

            var cleaned = cardNumber.Replace(" ", "");
            if (cleaned.Length < 1)
                return "Unknown";

            var firstDigit = cleaned[0];
            return firstDigit switch
            {
                '4' => "Visa",
                '5' => "Mastercard",
                '3' => "American Express",
                '6' => "Discover",
                _ => "Unknown"
            };
        }
    }
}
