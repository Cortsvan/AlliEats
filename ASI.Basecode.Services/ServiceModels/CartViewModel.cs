using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.ServiceModels
{
    public class CartViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount => CartItems.Sum(x => x.TotalPrice);
        public int TotalItems => CartItems.Sum(x => x.Quantity);
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}