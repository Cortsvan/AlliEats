using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    public class HomeController : ControllerBase<HomeController>
    {
        private readonly IMenuService _menuService;
        private readonly IReviewService _reviewService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="localizer"></param>
        /// <param name="mapper"></param>
        /// <param name="menuService"></param>
        /// <param name="reviewService"></param>
        /// <param name="orderService"></param>
        /// <param name="userService"></param>
        public HomeController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IMenuService menuService,
                              IReviewService reviewService,
                              IOrderService orderService,
                              IUserService userService,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _menuService = menuService;
            _reviewService = reviewService;
            _orderService = orderService;
            _userService = userService;
        }

        /// <summary>
        /// Returns Home View.
        /// </summary>
        /// <returns> Home View </returns>
        public IActionResult Index()
        {
            var user = _httpContextAccessor.HttpContext.User;
            var isAuthenticated = user.Identity.IsAuthenticated;
            var isAdmin = user.IsInRole("Admin");

            if (isAuthenticated && isAdmin)
            {
                // Fetch dashboard statistics for admin
                var allOrders = _orderService.GetAllOrders();
                var allUsers = _userService.GetAllUsers();
                var allReviews = _reviewService.GetAllReviews();

                // Today's orders
                var todayOrders = allOrders.Count(o => o.CreatedTime.Date == DateTime.Today);
                ViewBag.TodayOrders = todayOrders;

                // Active users (users who have placed orders)
                var activeUsers = allOrders.Select(o => o.UserId).Distinct().Count();
                ViewBag.ActiveUsers = activeUsers;

                // Total revenue
                var totalRevenue = allOrders
                    .Where(o => o.Status != "Cancelled" && o.Status != "Pending")
                    .Sum(o => o.TotalAmount); ViewBag.TotalRevenue = totalRevenue;

                // Average rating
                var averageRating = allReviews.Reviews.Any() 
                    ? allReviews.Reviews.Average(r => r.Rating) 
                    : 0;
                ViewBag.AverageRating = averageRating;
            }
            else if (isAuthenticated && !isAdmin)
            {
                // Only fetch menu items for authenticated non-admin users
                var activeMenuItems = _menuService.GetActiveMenuItems();
                // Take top 6 items for preview
                var featuredItems = activeMenuItems.Take(6).ToList();
                ViewBag.FeaturedMenuItems = featuredItems;

                // Get distinct categories for the category section
                var categories = activeMenuItems
                    .Select(m => m.Category)
                    .Distinct()
                    .Take(3)
                    .ToList();
                ViewBag.Categories = categories;

                // Get featured reviews (top rated reviews with comments)
                var allReviews = _reviewService.GetAllReviews();
                var featuredReviews = allReviews.Reviews
                    .Where(r => !string.IsNullOrEmpty(r.Comment) && r.Rating >= 4)
                    .OrderByDescending(r => r.Rating)
                    .ThenByDescending(r => r.ReviewDate)
                    .Take(6)
                    .ToList();
                ViewBag.FeaturedReviews = featuredReviews;
            }

            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
