using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Data.Models
{
    public class PaymentCard
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CardholderName { get; set; }

        [Required]
        [MaxLength(19)] // Format: 1234 5678 9012 3456
        public string CardNumber { get; set; }

        [Required]
        [MaxLength(5)] // Format: MM/YY
        public string ExpiryDate { get; set; }

        [Required]
        [MaxLength(4)]
        public string CVV { get; set; }

        [MaxLength(20)]
        public string CardType { get; set; } // Visa, Mastercard, etc.

        public bool IsDefault { get; set; } = false;

        [Required]
        public DateTime CreatedTime { get; set; }

        public DateTime? UpdatedTime { get; set; }

        [MaxLength(50)]
        public string CreatedBy { get; set; }

        [MaxLength(50)]
        public string UpdatedBy { get; set; }
    }
}
