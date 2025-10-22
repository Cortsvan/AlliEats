using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        public OrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<Order> GetOrdersByUserId(string userId)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedTime)
                .ToList();
        }

        public IEnumerable<Order> GetAllOrders()
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .OrderByDescending(o => o.CreatedTime)
                .ToList();
        }

        public Order GetOrderById(int id)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefault(o => o.Id == id);
        }

        public Order GetOrderByOrderNumber(string orderNumber)
        {
            return this.GetDbSet<Order>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefault(o => o.OrderNumber == orderNumber);
        }

        public void AddOrder(Order order)
        {
            this.GetDbSet<Order>().Add(order);
            UnitOfWork.SaveChanges();

            // Reload the order with navigation properties after saving
            // Use the Context property from BaseRepository
            Context.Entry(order)
                .Collection(o => o.OrderItems)
                .Load();
        }

        public void UpdateOrder(Order order)
        {
            this.GetDbSet<Order>().Update(order);
            UnitOfWork.SaveChanges();
        }

        public void DeleteOrder(int id)
        {
            var order = GetOrderById(id);
            if (order != null)
            {
                this.GetDbSet<Order>().Remove(order);
                UnitOfWork.SaveChanges();
            }
        }

        public bool OrderExists(int id)
        {
            return this.GetDbSet<Order>().Any(o => o.Id == id);
        }

        public bool OrderNumberExists(string orderNumber)
        {
            return this.GetDbSet<Order>().Any(o => o.OrderNumber == orderNumber);
        }

        public string GenerateOrderNumber()
        {
            string orderNumber;
            do
            {
                // Generate format: ORD-YYYYMMDD-XXXXX (where X is random number)
                var random = new Random();
                var dateStr = DateTime.Now.ToString("yyyyMMdd");
                var randomNum = random.Next(10000, 99999);
                orderNumber = $"ORD-{dateStr}-{randomNum}";
            }
            while (OrderNumberExists(orderNumber));

            return orderNumber;
        }
    }
}