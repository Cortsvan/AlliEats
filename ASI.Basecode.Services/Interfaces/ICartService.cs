using ASI.Basecode.Services.ServiceModels;

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
    }
}