using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class PaymentCardViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required(ErrorMessage = "Cardholder name is required")]
        [Display(Name = "Cardholder Name")]
        [StringLength(100, ErrorMessage = "Cardholder name cannot exceed 100 characters")]
        public string CardholderName { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        [Display(Name = "Card Number")]
        [RegularExpression(@"^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}$", ErrorMessage = "Invalid card number format")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        [Display(Name = "Expiry Date")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Invalid expiry date format (MM/YY)")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
        public string CVV { get; set; }

        [Display(Name = "Card Type")]
        public string CardType { get; set; }

        [Display(Name = "Set as Default")]
        public bool IsDefault { get; set; }

        public DateTime CreatedTime { get; set; }

        // Helper properties for display
        public string MaskedCardNumber
        {
            get
            {
                if (string.IsNullOrEmpty(CardNumber))
                    return string.Empty;

                var cleaned = CardNumber.Replace(" ", "");
                if (cleaned.Length < 4)
                    return "****";

                return $"**** **** **** {cleaned.Substring(cleaned.Length - 4)}";
            }
        }

        public string CardBrand
        {
            get
            {
                if (string.IsNullOrEmpty(CardNumber))
                    return "Unknown";

                var firstDigit = CardNumber.Replace(" ", "")[0];
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

        public bool IsExpired
        {
            get
            {
                if (string.IsNullOrEmpty(ExpiryDate))
                    return true;

                var parts = ExpiryDate.Split('/');
                if (parts.Length != 2)
                    return true;

                if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                    return true;

                var expiry = new DateTime(2000 + year, month, 1).AddMonths(1).AddDays(-1);
                return expiry < DateTime.Now;
            }
        }
    }
}
