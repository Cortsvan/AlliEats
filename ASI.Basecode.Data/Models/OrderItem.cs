using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Data.Models
{
    public partial class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string MenuItemName { get; set; } // Store name to avoid issues if menu item is deleted

        [Required]
        public decimal Price { get; set; } // Store price at time of order

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedTime { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual MenuItem MenuItem { get; set; }
    }
}