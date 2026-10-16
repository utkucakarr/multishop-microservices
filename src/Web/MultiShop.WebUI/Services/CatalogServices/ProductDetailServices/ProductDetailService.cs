using MultiShop.DtoLayer.CatalogDtos.ProductDetailDtos;
using System.Net;

namespace MultiShop.WebUI.Services.CatalogServices.ProductDetailServices
{
    public class ProductDetailService : IProductDetailService
    {
        private readonly HttpClient _httpClient;

        public ProductDetailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> CreateProductDetailAsync(CreateProductDetailDto createProductDetailDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("productdetails", createProductDetailDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<GetByIdProductDetailDto> GetByProductIdProductDetailAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("productdetails/GetProductDetailByProductId/" + id);
            // Detay kaydı girilmemiş ürünlerde Catalog 404 döner; sayfa boş açıklamayla gösterilir.
            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                return new GetByIdProductDetailDto();
            return await responseMessage.ReadOrThrowAsync<GetByIdProductDetailDto>();
        }

        public async Task<string?> UpdateProductDetailAsync(UpdateProductDetailDto updateProductDetailDto)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync("productdetails", updateProductDetailDto);
            return await responseMessage.GetErrorMessageAsync();
        }
    }
}
