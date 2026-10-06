using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.OrderServices.OrderDetailServices
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly HttpClient _httpClient;

        public OrderDetailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateOrderDetailAsync(CreateOrderDetailDto createOrderDetailDto)
        {
            var response = await _httpClient.PostAsJsonAsync<CreateOrderDetailDto>("orderDetails", createOrderDetailDto);
        }

        public async Task<List<GetOrderDetailByOrderIdDto>> GetOrderDetailByOrderingId(int orderingId)
        {
            var responseMessage = await _httpClient.GetAsync("orderDetails/GetOrderDetailById?id=" + orderingId);
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<GetOrderDetailByOrderIdDto>>(jsonData);
            return values;
        }
    }
}
