using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IMapper _mapper;

        public CartService(ICartRepository cartRepository, IMenuRepository menuRepository, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _menuRepository = menuRepository;
            _mapper = mapper;
        }

        public CartViewModel GetCartByUserId(string userId)
        {
            var cart = _cartRepository.GetCartByUserId(userId);
            if (cart == null)
            {
                return new CartViewModel { UserId = userId };
            }

            return _mapper.Map<CartViewModel>(cart);
        }

        public void AddToCart(string userId, int menuItemId, int quantity = 1)
        {
            var menuItem = _menuRepository.GetMenuItemById(menuItemId);
            if (menuItem == null || !menuItem.IsActive)
            {
                throw new InvalidOperationException("Menu item not found or not available.");
            }

            var cart = _cartRepository.GetCartByUserId(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedTime = DateTime.Now,
                    UpdatedTime = DateTime.Now
                };
                _cartRepository.CreateCart(cart);
            }

            // Check if item already exists in cart
            var existingCartItem = cart.CartItems.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
                existingCartItem.UpdatedTime = DateTime.Now;
                _cartRepository.UpdateCartItem(existingCartItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    MenuItemId = menuItemId,
                    Quantity = quantity,
                    Price = menuItem.Price,
                    CreatedTime = DateTime.Now,
                    UpdatedTime = DateTime.Now
                };
                _cartRepository.AddCartItem(cartItem);
            }

            cart.UpdatedTime = DateTime.Now;
            _cartRepository.UpdateCart(cart);
        }

        public void UpdateCartItem(int cartItemId, int quantity)
        {
            var cartItem = _cartRepository.GetCartItemById(cartItemId);
            if (cartItem == null)
            {
                throw new InvalidOperationException("Cart item not found.");
            }

            if (quantity <= 0)
            {
                _cartRepository.DeleteCartItem(cartItemId);
            }
            else
            {
                cartItem.Quantity = quantity;
                cartItem.UpdatedTime = DateTime.Now;
                _cartRepository.UpdateCartItem(cartItem);

                var cart = _cartRepository.GetCartById(cartItem.CartId);
                cart.UpdatedTime = DateTime.Now;
                _cartRepository.UpdateCart(cart);
            }
        }

        public void RemoveFromCart(int cartItemId)
        {
            var cartItem = _cartRepository.GetCartItemById(cartItemId);
            if (cartItem != null)
            {
                var cart = _cartRepository.GetCartById(cartItem.CartId);
                _cartRepository.DeleteCartItem(cartItemId);

                cart.UpdatedTime = DateTime.Now;
                _cartRepository.UpdateCart(cart);
            }
        }

        public void ClearCart(string userId)
        {
            var cart = _cartRepository.GetCartByUserId(userId);
            if (cart != null)
            {
                _cartRepository.DeleteCartItemsByCartId(cart.Id);
                cart.UpdatedTime = DateTime.Now;
                _cartRepository.UpdateCart(cart);
            }
        }

        public int GetCartItemCount(string userId)
        {
            var cart = _cartRepository.GetCartByUserId(userId);
            return cart?.CartItems.Sum(x => x.Quantity) ?? 0;
        }
    }
}