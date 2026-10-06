using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.StatisticServices.CatalogStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.CommentStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.DiscountStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.MessageStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.UserStatisticServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StatisticController : Controller
    {
        private readonly ICatalogStatisticService _catalogStatisticService;
        private readonly IUserStatisticService _userStatisticService;
        private readonly ICommentStatisticService _commentStatisticService;
        private readonly IDiscountStatisticService _discountStatisticService;
        private readonly IMessageStatisticService _messageStatisticService;

        public StatisticController(ICatalogStatisticService catalogStatisticService, IUserStatisticService userStatisticService, ICommentStatisticService commentStatisticService, IDiscountStatisticService discountStatisticService, IMessageStatisticService messageStatisticService)
        {
            _catalogStatisticService = catalogStatisticService;
            _userStatisticService = userStatisticService;
            _commentStatisticService = commentStatisticService;
            _discountStatisticService = discountStatisticService;
            _messageStatisticService = messageStatisticService;
        }

        public async Task<IActionResult> Index()
        {
            var getBrandCount = await _catalogStatisticService.GetBrandCountAsync();
            var getCategoryCount = await _catalogStatisticService.GetCategoryCountAsync();
            var getMaxPriceProductName = await _catalogStatisticService.GetMaxPriceProductNameAsync();
            var getMinPriceProductName = await _catalogStatisticService.GetMinPriceProductNameAsync();
            //var getProductAvgPrice = await _catalogStatisticService.GetProductAvgPriceAsync();
            var getProductCount = await _catalogStatisticService.GetProductCountAsync();
            var getUserCount = await _userStatisticService.GetUserCountAsync();
            var getActiveCommentCount = await _commentStatisticService.GetActiveTotalCommentCountAsync();
            var getPassiveCommentCount = await _commentStatisticService.GetPassiveTotalCommentCountAsync();
            var getTotalCommentCount = await _commentStatisticService.GetTotalCommentCountAsync();
            var getDiscountCouponCount = await _discountStatisticService.GetDiscountCouponCountAsync();
            var getTotalMessageCount = await _messageStatisticService.GetTotalMessageCountAsync();

            ViewBag.getBrandCount = getBrandCount;
            ViewBag.getCategoryCount = getCategoryCount;
            ViewBag.getMaxPriceProductName = getMaxPriceProductName;
            ViewBag.getMinPriceProductName = getMinPriceProductName;
            //ViewBag.getProductAvgPrice = getProductAvgPrice;
            ViewBag.getProductCount = getProductCount;
            ViewBag.getUserCount = getUserCount;
            ViewBag.getActiveCommentCount = getActiveCommentCount;
            ViewBag.getPassiveCommentCount = getPassiveCommentCount;
            ViewBag.getTotalCommentCount = getTotalCommentCount;
            ViewBag.getDiscountCouponCount = getDiscountCouponCount;
            ViewBag.getTotalMessageCount = getTotalMessageCount;

            return View();
        }

    }
}
