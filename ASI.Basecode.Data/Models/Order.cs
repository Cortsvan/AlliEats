using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Data.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string OrderNumber { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string PaymentMethod { get; set; } // Cash, Card, GCash, etc.

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Preparing, Ready, Delivered, Cancelled

        [MaxLength(500)]
        public string Notes { get; set; }

        [Required]
        public DateTime CreatedTime { get; set; }

        public DateTime? UpdatedTime { get; set; }

        [MaxLength(50)]
        public string CreatedBy { get; set; }

        [MaxLength(50)]
        public string UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}