using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Services.Interfaces;

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
        /// <param name="menuService"></param>
        /// <param name="reviewService"></param>
        /// <param name="orderService"></param>
        /// <param name="userService"></param>
        /// <param name="mapper"></param>
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
                ViewBag.TodayOrders = _orderService.GetTodayOrdersCount();
                ViewBag.ActiveUsers = _orderService.GetActiveUsersCount();
                ViewBag.TotalRevenue = _orderService.GetTotalRevenue();
                ViewBag.AverageRating = _reviewService.GetAverageRating();
            }
            else if (isAuthenticated && !isAdmin)
            {
                ViewBag.FeaturedMenuItems = _menuService.GetFeaturedMenuItems(6);
                ViewBag.Categories = _menuService.GetTopCategories(3);
                ViewBag.FeaturedReviews = _reviewService.GetFeaturedReviews(6).Reviews;
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}