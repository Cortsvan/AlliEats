using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetOrdersByUserId(string userId);
        IEnumerable<Order> GetAllOrders();
        Order GetOrderById(int id);
        Order GetOrderByOrderNumber(string orderNumber);
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int id);
        bool OrderExists(int id);
        bool OrderNumberExists(string orderNumber);
        string GenerateOrderNumber();
    }
}