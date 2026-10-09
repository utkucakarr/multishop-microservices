using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Services.MessageServices.ContactMessageServices;
using MultiShop.WebUI.Services.StatisticServices.CatalogStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.CommentStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.DiscountStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.UserStatisticServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    public class StatisticController : Controller
    {
        private readonly ICatalogStatisticService _catalogStatisticService;
        private readonly IUserStatisticService _userStatisticService;
        private readonly ICommentStatisticService _commentStatisticService;
        private readonly IDiscountStatisticService _discountStatisticService;
        private readonly IContactMessageService _contactMessageService;

        public StatisticController(ICatalogStatisticService catalogStatisticService, IUserStatisticService userStatisticService, ICommentStatisticService commentStatisticService, IDiscountStatisticService discountStatisticService, IContactMessageService contactMessageService)
        {
            _catalogStatisticService = catalogStatisticService;
            _userStatisticService = userStatisticService;
            _commentStatisticService = commentStatisticService;
            _discountStatisticService = discountStatisticService;
            _contactMessageService = contactMessageService;
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
            var getTotalMessageCount = (await _contactMessageService.GetCountAsync()).Total;

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
