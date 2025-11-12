using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class ReviewRepository : BaseRepository, IReviewRepository
    {
        public ReviewRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public Review GetReviewByOrderId(int orderId)
        {
            return this.GetDbSet<Review>()
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefault(r => r.OrderId == orderId);
        }

        public IEnumerable<Review> GetReviewsByUserId(string userId)
        {
            return this.GetDbSet<Review>()
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedTime)
                .ToList();
        }

        public IEnumerable<Review> GetAllReviews()
        {
            return this.GetDbSet<Review>()
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                .OrderByDescending(r => r.CreatedTime)
                .ToList();
        }

        public void AddReview(Review review)
        {
            this.GetDbSet<Review>().Add(review);
            UnitOfWork.SaveChanges();
        }

        public void UpdateReview(Review review)
        {
            this.GetDbSet<Review>().Update(review);
            UnitOfWork.SaveChanges();
        }

        public void DeleteReview(int id)
        {
            var review = this.GetDbSet<Review>().Find(id);
            if (review != null)
            {
                this.GetDbSet<Review>().Remove(review);
                UnitOfWork.SaveChanges();
            }
        }

        public bool ReviewExists(int orderId)
        {
            return this.GetDbSet<Review>().Any(r => r.OrderId == orderId);
        }

        public bool CanUserReviewOrder(int orderId, string userId)
        {
            var order = this.GetDbSet<Order>()
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return false;

            // Only allow reviews for delivered orders
            if (order.Status != "Received") return false;

            // Check if review already exists
            if (ReviewExists(orderId)) return false;

            return true;
        }
    }
}