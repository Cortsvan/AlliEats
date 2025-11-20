using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IOrderService
    {
        IEnumerable<OrderViewModel> GetOrdersByUserId(string userId);
        IEnumerable<OrderViewModel> GetAllOrders();
        IEnumerable<OrderViewModel> GetAllOrders(string status);
        OrderViewModel GetOrderById(int id);
        OrderViewModel GetOrderByOrderNumber(string orderNumber);
        OrderViewModel CreateOrderFromCart(string userId, CheckoutViewModel checkoutModel);
        void UpdateOrderStatus(int orderId, string status);
        void CancelOrder(int orderId);
        bool OrderExists(int id);
        CheckoutViewModel PrepareCheckout(string userId);

        //Dashboard methods
        int GetTodayOrdersCount();
        int GetActiveUsersCount();
        decimal GetTotalRevenue();

        //Validation methods
        bool IsValidOrderStatus(string status);
        IEnumerable<string> GetValidOrderStatuses();
        (bool IsValid, string Message) ValidateStatusUpdate(int orderId, string newStatus);
        string GetStatusUpdateMessage(string status);

        //customer order validation methods
        bool ValidateOrderOwnership(int orderId, string userId);
        (bool CanConfirm, string Message) ValidateReceiptConfirmation(int orderId, string userId);
        (bool CanCancel, string Message) ValidateOrderCancellation(int orderId, string userId);
    }
}