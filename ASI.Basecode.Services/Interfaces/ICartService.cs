using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ICartService
    {
        CartViewModel GetCartByUserId(string userId);
        void AddToCart(string userId, int menuItemId, int quantity = 1);
        void UpdateCartItem(int cartItemId, int quantity);
        void RemoveFromCart(int cartItemId);
        void ClearCart(string userId);
        int GetCartItemCount(string userId);

        // Validation Methods
        StockValidationResult ValidateCartStock(string userId);
        CartStockFixResult AutoFixStockIssues(string userId);
        bool ValidateCartItemOwnership(int cartItemId, string userId);
    }

    // for stock validation results
    public class StockValidationResult
    {
        public bool HasIssues { get; set; }
        public List<StockIssue> StockIssues { get; set; } = new List<StockIssue>();
        public string Message { get; set; }
    }

    public class StockIssue
    {
        public int CartItemId { get; set; }
        public int MenuItemId { get; set; }
        public string ItemName { get; set; }
        public int RequestedQuantity { get; set; }
        public int AvailableStock { get; set; }
        public string Issue { get; set; } // "unavailable", "out-of-stock", "insufficient"
        public string Message { get; set; }
    }

    public class CartStockFixResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<FixedItem> FixedItems { get; set; } = new List<FixedItem>();
        public List<RemovedItem> RemovedItems { get; set; } = new List<RemovedItem>();
    }

    public class FixedItem
    {
        public string Name { get; set; }
        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
    }

    public class RemovedItem
    {
        public string Name { get; set; }
        public string Reason { get; set; }
    }
}