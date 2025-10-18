using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class CartRepository : BaseRepository, ICartRepository
    {
        public CartRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public Cart GetCartByUserId(string userId)
        {
            return this.GetDbSet<Cart>()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MenuItem)
                .FirstOrDefault(c => c.UserId == userId);
        }

        public Cart GetCartById(int cartId)
        {
            return this.GetDbSet<Cart>()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MenuItem)
                .FirstOrDefault(c => c.Id == cartId);
        }

        public void CreateCart(Cart cart)
        {
            this.GetDbSet<Cart>().Add(cart);
            UnitOfWork.SaveChanges();
        }

        public void UpdateCart(Cart cart)
        {
            this.GetDbSet<Cart>().Update(cart);
            UnitOfWork.SaveChanges();
        }

        public void DeleteCart(int cartId)
        {
            var cart = this.GetDbSet<Cart>().Find(cartId);
            if (cart != null)
            {
                this.GetDbSet<Cart>().Remove(cart);
                UnitOfWork.SaveChanges();
            }
        }

        public CartItem GetCartItemById(int cartItemId)
        {
            return this.GetDbSet<CartItem>()
                .Include(ci => ci.MenuItem)
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.Id == cartItemId);
        }

        public void AddCartItem(CartItem cartItem)
        {
            this.GetDbSet<CartItem>().Add(cartItem);
            UnitOfWork.SaveChanges();
        }

        public void UpdateCartItem(CartItem cartItem)
        {
            this.GetDbSet<CartItem>().Update(cartItem);
            UnitOfWork.SaveChanges();
        }

        public void DeleteCartItem(int cartItemId)
        {
            var cartItem = this.GetDbSet<CartItem>().Find(cartItemId);
            if (cartItem != null)
            {
                this.GetDbSet<CartItem>().Remove(cartItem);
                UnitOfWork.SaveChanges();
            }
        }

        public void DeleteCartItemsByCartId(int cartId)
        {
            var cartItems = this.GetDbSet<CartItem>().Where(ci => ci.CartId == cartId);
            this.GetDbSet<CartItem>().RemoveRange(cartItems);
            UnitOfWork.SaveChanges();
        }

        public bool CartExists(string userId)
        {
            return this.GetDbSet<Cart>().Any(c => c.UserId == userId);
        }

        public bool CartItemExists(int cartId, int menuItemId)
        {
            return this.GetDbSet<CartItem>().Any(ci => ci.CartId == cartId && ci.MenuItemId == menuItemId);
        }
    }
}