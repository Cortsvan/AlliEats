using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ICartRepository
    {
        Cart GetCartByUserId(string userId);
        Cart GetCartById(int cartId);
        void CreateCart(Cart cart);
        void UpdateCart(Cart cart);
        void DeleteCart(int cartId);

        CartItem GetCartItemById(int cartItemId);
        void AddCartItem(CartItem cartItem);
        void UpdateCartItem(CartItem cartItem);
        void DeleteCartItem(int cartItemId);
        void DeleteCartItemsByCartId(int cartId);

        bool CartExists(string userId);
        bool CartItemExists(int cartId, int menuItemId);
    }
}