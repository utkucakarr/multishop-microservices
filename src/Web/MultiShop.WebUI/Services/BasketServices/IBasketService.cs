using MultiShop.DtoLayer.BasketDtos;

namespace MultiShop.WebUI.Services.BasketServices
{
    public interface IBasketService
    {
        Task<BasketTotalDto> GetBasket();

        Task SaveBasket(BasketTotalDto basketTotalDto);

        Task DeleteBasket(string userId);

        Task AddBasketItem(BasketItemDto basketItemDto);

        Task<bool> RemoveAllBasketItem(string productId);

        Task RemoveBasketItem(string productId);

        Task AddBasketBtnItem(BasketItemDto basketItemDto);
    }
}
