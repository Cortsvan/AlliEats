using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ASI.Basecode.Services.ServiceModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string OrderNumber { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        public string Status { get; set; }

        [Display(Name = "Order Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }

        [Display(Name = "Order Date")]
        public DateTime CreatedTime { get; set; }

        public DateTime? UpdatedTime { get; set; }

        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();

        // Helper properties
        public int TotalItems => OrderItems?.Sum(x => x.Quantity) ?? 0;
        public string StatusBadgeClass => Status?.ToLower() switch
        {
            "pending" => "bg-warning text-dark",
            "confirmed" => "bg-info",
            "preparing" => "bg-primary",
            "ready" => "bg-success",
            "delivered" => "bg-success",
            "cancelled" => "bg-danger",
            _ => "bg-secondary"
        };
    }

    public class OrderItemViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Order Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }

        // Cart summary for display
        public CartViewModel Cart { get; set; }

        // Available payment methods
        public List<string> PaymentMethods { get; set; } = new List<string>
        {
            "Cash on Delivery",
            "Credit Card",
            "Debit Card",
            "GCash",
            "PayMaya"
        };
    }
}