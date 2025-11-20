using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Services.Interfaces;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    public class HomeController : ControllerBase<HomeController>
    {
        private readonly IMenuService _menuService;
        private readonly IReviewService _reviewService;

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
        public HomeController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IMenuService menuService,
                              IReviewService reviewService,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _menuService = menuService;
            _reviewService = reviewService;
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

            // Only fetch menu items for authenticated non-admin users
            if (isAuthenticated && !isAdmin)
            {
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
