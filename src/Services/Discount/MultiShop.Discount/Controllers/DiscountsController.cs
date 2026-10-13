using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Discount.Dtos;
using MultiShop.Discount.Services;
using System.Runtime.CompilerServices;

namespace MultiShop.Discount.Controllers
{
    // Kuponu sepete uygularken Basket doğrular (validate, kullanıcının token'ıyla); kupon listesi, ekleme, güncelleme, silme Admin.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountsController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet]
        public async Task<IActionResult> DiscountCouponList()
        {
            var values = await _discountService.GetAllDiscountCouponAsync();
            return Ok(values);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiscountCouponById(int id)
        {
            var values = await _discountService.GetByIdDiscountCouponAsync(id);
            return Ok(values);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateDiscountCoupon(CreateDiscountCouponDto createCouponDto)
        {
            await _discountService.CreateDiscountCouponAsync(createCouponDto);
            return Ok("Kupon başarıyla oluşturuldu");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public async Task<IActionResult> DeleteDiscountCoupon(int id)
        {
            await _discountService.DeleteDiscountCouponAsync(id);
            return Ok("Kupon başarıyla silindi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public async Task<IActionResult> UpdateDiscountCoupon(UpdateDiscountCouponDto updateCouponDto)
        {
            await _discountService.UpdateDiscountCouponAync(updateCouponDto);
            return Ok("İndirim kuponu başarıyla güncellendi");
        }

        /// <summary>Sepette kupon uygulanırken Basket servisi çağırır: geçerliyse kod ve oran, değilse 404.</summary>
        [Authorize(Policy = MultiShopPolicies.Authenticated)]
        [HttpGet("validate/{code}")]
        public async Task<IActionResult> ValidateCoupon(string code)
        {
            var value = await _discountService.ValidateCouponAsync(code);
            return Ok(value);
        }
    }
}