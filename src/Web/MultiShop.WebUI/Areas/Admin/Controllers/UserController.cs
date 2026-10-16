using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Services.OrderServices.OrderAddressServices;
using MultiShop.WebUI.Services.UserIdentityServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IUserIdentityService _userIdentityService;
        private readonly IOrderAddressService _orderAddressService;

        public UserController(IUserIdentityService userIdentityService, IOrderAddressService orderAddressService)
        {
            _userIdentityService = userIdentityService;
            _orderAddressService = orderAddressService;
        }

        public async Task<IActionResult> UserList()
        {
            var values = await _userIdentityService.GetAllUserListAsync();
            return View(values);
        }


        /// <summary>Kullanıcının sipariş adresleri (Order). Önceden Cargo'daki, hiçbir akışın doldurmadığı CargoCustomer kaydını gösteriyordu.</summary>
        public async Task<IActionResult> UserAddressInfo(string id)
        {
            var values = await _orderAddressService.GetAddressesByUserIdAsync(id);
            return View(values);
        }
    }
}
