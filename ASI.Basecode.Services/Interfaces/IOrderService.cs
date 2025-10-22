using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IOrderService
    {
        IEnumerable<OrderViewModel> GetOrdersByUserId(string userId);
        IEnumerable<OrderViewModel> GetAllOrders();
        OrderViewModel GetOrderById(int id);
        OrderViewModel GetOrderByOrderNumber(string orderNumber);
        OrderViewModel CreateOrderFromCart(string userId, CheckoutViewModel checkoutModel);
        void UpdateOrderStatus(int orderId, string status);
        void CancelOrder(int orderId);
        bool OrderExists(int id);
        CheckoutViewModel PrepareCheckout(string userId);
    }
}