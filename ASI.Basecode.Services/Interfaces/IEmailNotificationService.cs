using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IEmailNotificationService
    {
        Task<bool> SendOrderConfirmationEmailAsync(string email, string customerName, string orderNumber, decimal totalAmount);
        Task<bool> SendOrderStatusUpdateEmailAsync(string email, string customerName, string orderNumber, string oldStatus, string newStatus);
        Task<bool> SendOrderReadyNotificationEmailAsync(string email, string customerName, string orderNumber);
        Task<bool> SendOrderDeliveredNotificationEmailAsync(string email, string customerName, string orderNumber);
        Task<bool> SendOrderCancelledNotificationEmailAsync(string email, string customerName, string orderNumber, string reason);
    }
}