using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int MenuItemId { get; set; }

        [Required]
        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        public int Quantity { get; set; }

        public decimal Price { get; set; }
        public decimal TotalPrice => Price * Quantity;

        // Menu item details
        public string MenuItemName { get; set; }
        public string MenuItemDescription { get; set; }
        public string MenuItemCategory { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}