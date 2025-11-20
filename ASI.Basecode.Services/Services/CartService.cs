using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
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

            if (!_menuRepository.HasSufficientStock(menuItemId, quantity))
            {
                throw new InvalidOperationException("Insufficient stock. Only " + menuItem.Stock + " items available.");
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
                var totalQuantity = existingCartItem.Quantity + quantity;

                if (!_menuRepository.HasSufficientStock(menuItemId, totalQuantity))
                {
                    throw new InvalidOperationException("Cannot add " + quantity + " more items. Only " + (menuItem.Stock - existingCartItem.Quantity) + " more items available.");
                }

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
                if (!_menuRepository.HasSufficientStock(cartItem.MenuItemId, quantity))
                {
                    var menuItem = _menuRepository.GetMenuItemById(cartItem.MenuItemId);
                    throw new InvalidOperationException("Insufficient stock. Only " + menuItem?.Stock + " items available.");
                }
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

        // validation methods
        public StockValidationResult ValidateCartStock(string userId)
        {
            try
            {
                var result = new StockValidationResult();

                var cart = GetCartByUserId(userId);
                if (cart == null || !cart.CartItems.Any())
                {
                    result.Message = "Cart is empty.";
                    return result;
                }

                foreach (var cartItem in cart.CartItems)
                {
                    var menuItem = _menuRepository.GetMenuItemById(cartItem.MenuItemId);

                    if (menuItem == null || !menuItem.IsActive)
                    {
                        result.StockIssues.Add(new StockIssue
                        {
                            CartItemId = cartItem.Id,
                            MenuItemId = cartItem.MenuItemId,
                            ItemName = cartItem.MenuItemName,
                            RequestedQuantity = cartItem.Quantity,
                            AvailableStock = 0,
                            Issue = "unavailable",
                            Message = $"{cartItem.MenuItemName} is no longer available"
                        });
                        result.HasIssues = true;
                    }
                    else if (menuItem.Stock < cartItem.Quantity)
                    {
                        result.StockIssues.Add(new StockIssue
                        {
                            CartItemId = cartItem.Id,
                            MenuItemId = cartItem.MenuItemId,
                            ItemName = cartItem.MenuItemName,
                            RequestedQuantity = cartItem.Quantity,
                            AvailableStock = menuItem.Stock,
                            Issue = menuItem.Stock == 0 ? "out-of-stock" : "insufficient",
                            Message = menuItem.Stock == 0
                                ? $"{cartItem.MenuItemName} is now out of stock"
                                : $"Only {menuItem.Stock} {cartItem.MenuItemName} available (you have {cartItem.Quantity} in cart)"
                        });
                        result.HasIssues = true;
                    }
                }

                result.Message = result.HasIssues ? "Stock issues detected" : "All items are available";
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating cart stock for user {userId}: {ex.Message}", ex);
            }
        }

        public CartStockFixResult AutoFixStockIssues(string userId)
        {
            try
            {
                var result = new CartStockFixResult { Success = true };

                var cart = GetCartByUserId(userId);
                if (cart == null || !cart.CartItems.Any())
                {
                    result.Message = "Cart is empty.";
                    return result;
                }

                var cartItemsCopy = cart.CartItems.ToList();

                foreach (var cartItem in cartItemsCopy)
                {
                    var menuItem = _menuRepository.GetMenuItemById(cartItem.MenuItemId);

                    if (menuItem == null || !menuItem.IsActive || menuItem.Stock == 0)
                    {
                        RemoveFromCart(cartItem.Id);
                        result.RemovedItems.Add(new RemovedItem
                        {
                            Name = cartItem.MenuItemName,
                            Reason = "no longer available"
                        });
                    }
                    else if (menuItem.Stock < cartItem.Quantity)
                    {
                        // Adjust quantity to available stock
                        UpdateCartItem(cartItem.Id, menuItem.Stock);
                        result.FixedItems.Add(new FixedItem
                        {
                            Name = cartItem.MenuItemName,
                            OldQuantity = cartItem.Quantity,
                            NewQuantity = menuItem.Stock
                        });
                    }
                }

                result.Message = "Stock issues have been resolved";
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error auto-fixing stock issues for user {userId}: {ex.Message}", ex);
            }
        }

        public bool ValidateCartItemOwnership(int cartItemId, string userId)
        {
            try
            {
                var cartItem = _cartRepository.GetCartItemById(cartItemId);
                if (cartItem == null) return false;

                var cart = _cartRepository.GetCartById(cartItem.CartId);
                return cart?.UserId == userId;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}