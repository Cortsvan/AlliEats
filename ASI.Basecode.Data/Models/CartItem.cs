using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Data.Models
{
    public partial class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }

        public int MenuItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public decimal Price { get; set; } // Store price at time of adding to cart

        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual Cart Cart { get; set; }
        public virtual MenuItem MenuItem { get; set; }
    }
}