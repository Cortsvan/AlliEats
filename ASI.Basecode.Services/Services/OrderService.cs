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
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, ICartService cartService, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _cartService = cartService;
            _mapper = mapper;
        }

        public IEnumerable<OrderViewModel> GetOrdersByUserId(string userId)
        {
            try
            {
                var orders = _orderRepository.GetOrdersByUserId(userId);

                if (orders == null || !orders.Any())
                {
                    return new List<OrderViewModel>();
                }

                // Manual mapping to avoid AutoMapper issues
                var orderViewModels = orders.Select(order => new OrderViewModel
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod,
                    Status = order.Status,
                    Notes = order.Notes,
                    CreatedTime = order.CreatedTime,
                    UpdatedTime = order.UpdatedTime,
                    OrderItems = order.OrderItems?.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItemName,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                        TotalPrice = oi.TotalPrice,
                        CreatedTime = oi.CreatedTime
                    }).ToList() ?? new List<OrderItemViewModel>()
                }).ToList();

                return orderViewModels;
            }
            catch (Exception ex)
            {
                // Log the error and return empty list
                throw new Exception($"Error retrieving orders for user {userId}: {ex.Message}", ex);
            }
        }

        public IEnumerable<OrderViewModel> GetAllOrders()
        {
            var orders = _orderRepository.GetAllOrders();
            return _mapper.Map<IEnumerable<OrderViewModel>>(orders);
        }

        public OrderViewModel GetOrderById(int id)
        {
            try
            {
                var order = _orderRepository.GetOrderById(id);
                if (order == null) return null;

                // Manual mapping to avoid AutoMapper issues
                return new OrderViewModel
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod,
                    Status = order.Status,
                    Notes = order.Notes,
                    CreatedTime = order.CreatedTime,
                    UpdatedTime = order.UpdatedTime,
                    OrderItems = order.OrderItems?.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItemName,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                        TotalPrice = oi.TotalPrice,
                        CreatedTime = oi.CreatedTime
                    }).ToList() ?? new List<OrderItemViewModel>()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving order {id}: {ex.Message}", ex);
            }
        }

        public OrderViewModel GetOrderByOrderNumber(string orderNumber)
        {
            var order = _orderRepository.GetOrderByOrderNumber(orderNumber);
            return _mapper.Map<OrderViewModel>(order);
        }

        public CheckoutViewModel PrepareCheckout(string userId)
        {
            var cart = _cartService.GetCartByUserId(userId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty. Cannot proceed to checkout.");
            }

            return new CheckoutViewModel
            {
                Cart = cart
            };
        }

        public OrderViewModel CreateOrderFromCart(string userId, CheckoutViewModel checkoutModel)
        {
            var cart = _cartService.GetCartByUserId(userId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty. Cannot create order.");
            }

            var order = new Order
            {
                UserId = userId,
                OrderNumber = _orderRepository.GenerateOrderNumber(),
                TotalAmount = cart.TotalAmount,
                PaymentMethod = checkoutModel.PaymentMethod,
                Status = "Pending",
                Notes = checkoutModel.Notes,
                CreatedTime = DateTime.Now,
                CreatedBy = userId
            };

            // Create order items from cart items
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    MenuItemId = cartItem.MenuItemId,
                    MenuItemName = cartItem.MenuItemName,
                    Price = cartItem.Price,
                    Quantity = cartItem.Quantity,
                    TotalPrice = cartItem.TotalPrice,
                    CreatedTime = DateTime.Now
                };

                order.OrderItems.Add(orderItem);
            }

            // Save the order
            _orderRepository.AddOrder(order);

            // Clear the cart after successful order creation
            _cartService.ClearCart(userId);

            // Create a simple OrderViewModel manually to avoid AutoMapper issues
            var orderViewModel = new OrderViewModel
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status,
                Notes = order.Notes,
                CreatedTime = order.CreatedTime,
                UpdatedTime = order.UpdatedTime,
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    MenuItemId = oi.MenuItemId,
                    MenuItemName = oi.MenuItemName,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.TotalPrice,
                    CreatedTime = oi.CreatedTime
                }).ToList()
            };

            return orderViewModel;
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            order.Status = status;
            order.UpdatedTime = DateTime.Now;
            order.UpdatedBy = System.Environment.UserName;

            _orderRepository.UpdateOrder(order);
        }

        public void CancelOrder(int orderId)
        {
            UpdateOrderStatus(orderId, "Cancelled");
        }

        public bool OrderExists(int id)
        {
            return _orderRepository.OrderExists(id);
        }
    }
}