using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly ICartService _cartService;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IMenuRepository menuRepository, ICartService cartService, IReviewRepository reviewRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _menuRepository = menuRepository;
            _cartService = cartService;
            _reviewRepository = reviewRepository;
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

                // Manual mapping to include menu item image paths and review status
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
                    HasReview = _reviewRepository.ReviewExists(order.Id),
                    OrderItems = order.OrderItems?.Select(oi =>
                    {
                        var menuItem = _menuRepository.GetMenuItemById(oi.MenuItemId);
                        return new OrderItemViewModel
                        {
                            Id = oi.Id,
                            OrderId = oi.OrderId,
                            MenuItemId = oi.MenuItemId,
                            MenuItemName = oi.MenuItemName,
                            Price = oi.Price,
                            Quantity = oi.Quantity,
                            TotalPrice = oi.TotalPrice,
                            CreatedTime = oi.CreatedTime,
                            MenuItemImagePath = menuItem?.ImagePath
                        };
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
            try
            {
                var orders = _orderRepository.GetAllOrders();

                if (orders == null || !orders.Any())
                {
                    return new List<OrderViewModel>();
                }

                // Manual mapping to include menu item image paths and review status
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
                    HasReview = _reviewRepository.ReviewExists(order.Id),
                    OrderItems = order.OrderItems?.Select(oi =>
                    {
                        var menuItem = _menuRepository.GetMenuItemById(oi.MenuItemId);
                        return new OrderItemViewModel
                        {
                            Id = oi.Id,
                            OrderId = oi.OrderId,
                            MenuItemId = oi.MenuItemId,
                            MenuItemName = oi.MenuItemName,
                            Price = oi.Price,
                            Quantity = oi.Quantity,
                            TotalPrice = oi.TotalPrice,
                            CreatedTime = oi.CreatedTime,
                            MenuItemImagePath = menuItem?.ImagePath
                        };
                    }).ToList() ?? new List<OrderItemViewModel>()
                }).ToList();

                return orderViewModels.OrderByDescending(o => o.CreatedTime);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all orders: {ex.Message}", ex);
            }
        }
        //overload method to allow status check
        public IEnumerable<OrderViewModel> GetAllOrders(string status)
        {
            try
            {
                var orders = _orderRepository.GetAllOrders().Where(o => o.Status == status).ToList(); ;

                if (orders == null || !orders.Any())
                {
                    return new List<OrderViewModel>();
                }

                // Manual mapping to include menu item image paths and review status
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
                    HasReview = _reviewRepository.ReviewExists(order.Id),
                    OrderItems = order.OrderItems?.Select(oi =>
                    {
                        var menuItem = _menuRepository.GetMenuItemById(oi.MenuItemId);
                        return new OrderItemViewModel
                        {
                            Id = oi.Id,
                            OrderId = oi.OrderId,
                            MenuItemId = oi.MenuItemId,
                            MenuItemName = oi.MenuItemName,
                            Price = oi.Price,
                            Quantity = oi.Quantity,
                            TotalPrice = oi.TotalPrice,
                            CreatedTime = oi.CreatedTime,
                            MenuItemImagePath = menuItem?.ImagePath
                        };
                    }).ToList() ?? new List<OrderItemViewModel>()
                }).ToList();

                return orderViewModels.OrderByDescending(o => o.CreatedTime);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all orders: {ex.Message}", ex);
            }
        }

        public OrderViewModel GetOrderById(int id)
        {
            try
            {
                var order = _orderRepository.GetOrderById(id);
                if (order == null) return null;

                // Manual mapping to include menu item image paths and review status
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
                    HasReview = _reviewRepository.ReviewExists(order.Id),
                    OrderItems = order.OrderItems?.Select(oi =>
                    {
                        var menuItem = _menuRepository.GetMenuItemById(oi.MenuItemId);
                        return new OrderItemViewModel
                        {
                            Id = oi.Id,
                            OrderId = oi.OrderId,
                            MenuItemId = oi.MenuItemId,
                            MenuItemName = oi.MenuItemName,
                            Price = oi.Price,
                            Quantity = oi.Quantity,
                            TotalPrice = oi.TotalPrice,
                            CreatedTime = oi.CreatedTime,
                            MenuItemImagePath = menuItem?.ImagePath
                        };
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

            foreach (var cartItem in cart.CartItems)
            {
                if (!_menuRepository.HasSufficientStock(cartItem.MenuItemId, cartItem.Quantity))
                {
                    var menuItem = _menuRepository.GetMenuItemById(cartItem.MenuItemId);
                    throw new InvalidOperationException($"Insufficient stock for {cartItem.MenuItemName}. Only {menuItem?.Stock ?? 0} items available.");
                }
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
                _menuRepository.DecrementStock(cartItem.MenuItemId, cartItem.Quantity);
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
                HasReview = false,
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

        //dashboard methods
        public int GetTodayOrdersCount()
        {
            try
            {
                var allOrders = GetAllOrders();
                return allOrders.Count(o => o.CreatedTime.Date == DateTime.Today);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting today's orders count: {ex.Message}", ex);
            }
        }

        public int GetActiveUsersCount()
        {
            try
            {
                var allOrders = GetAllOrders();
                return allOrders.Select(o => o.UserId).Distinct().Count();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting active users count: {ex.Message}", ex);
            }
        }

        public decimal GetTotalRevenue()
        {
            try
            {
                var allOrders = GetAllOrders();
                return allOrders
                    .Where(o => o.Status != "Cancelled" && o.Status != "Pending")
                    .Sum(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating total revenue: {ex.Message}", ex);
            }
        }

        //Validation Methods
        public bool IsValidOrderStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "On the Way", "Received", "Cancelled" };
            return validStatuses.Contains(status);
        }

        public IEnumerable<string> GetValidOrderStatuses()
        {
            return new[] { "Pending", "Confirmed", "Preparing", "Ready", "On the Way", "Received", "Cancelled" };
        }

        public (bool IsValid, string Message) ValidateStatusUpdate(int orderId, string newStatus)
        {
            try
            {
                if (!IsValidOrderStatus(newStatus))
                {
                    return (false, "Invalid status selected.");
                }

                var order = _orderRepository.GetOrderById(orderId);
                if (order == null)
                {
                    return (false, "Order not found.");
                }

                if (order.Status == "Received" || order.Status == "Cancelled")
                {
                    return (false, "Cannot update status of completed or cancelled orders.");
                }

                return (true, "Status update is valid.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating status update for order {orderId}: {ex.Message}", ex);
            }
        }

        public string GetStatusUpdateMessage(string status)
        {
            return status switch
            {
                "On the Way" => $"Order status updated to {status}. The order will be automatically marked as 'Received' after 2 hours if not confirmed by the customer.",
                _ => $"Order status updated to {status} successfully."
            };
        }


        // customer order validation methods
        public bool ValidateOrderOwnership(int orderId, string userId)
        {
            try
            {
                var order = _orderRepository.GetOrderById(orderId);
                return order != null && order.UserId == userId;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public (bool CanConfirm, string Message) ValidateReceiptConfirmation(int orderId, string userId)
        {
            try
            {
                var order = _orderRepository.GetOrderById(orderId);

                if (order == null)
                {
                    return (false, "Order not found.");
                }

                if (order.UserId != userId)
                {
                    return (false, "Access denied.");
                }

                if (order.Status != "On the Way")
                {
                    return (false, $"Order cannot be confirmed at this time. Current status: {order.Status}");
                }

                return (true, "Order can be confirmed.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating receipt confirmation for order {orderId}: {ex.Message}", ex);
            }
        }

        public (bool CanCancel, string Message) ValidateOrderCancellation(int orderId, string userId)
        {
            try
            {
                var order = _orderRepository.GetOrderById(orderId);

                if (order == null)
                {
                    return (false, "Order not found.");
                }

                if (order.UserId != userId)
                {
                    return (false, "Access denied.");
                }

                if (order.Status != "Pending")
                {
                    return (false, $"Order cannot be cancelled at this time. Current status: {order.Status}. Orders can only be cancelled while pending.");
                }

                return (true, "Order can be cancelled.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating order cancellation for order {orderId}: {ex.Message}", ex);
            }
        }
    }
}