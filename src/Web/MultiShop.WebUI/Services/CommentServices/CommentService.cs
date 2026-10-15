using MultiShop.DtoLayer.CommentDtos;

namespace MultiShop.WebUI.Services.CommentServices
{
    public class CommentService : ICommentService
    {
        private readonly HttpClient _httpClient;

        public CommentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ResultProductCommentDto>> CommentListByProductId(string productId)
            => await _httpClient.GetFromJsonAsync<List<ResultProductCommentDto>>($"comments/product/{Uri.EscapeDataString(productId)}") ?? new();

        public async Task<string?> CreateCommentAsync(CreateCommentDto createCommentDto)
            => await (await _httpClient.PostAsJsonAsync("comments", createCommentDto)).GetErrorMessageAsync();

        public async Task<CommentPageDto> GetCommentsAsync(string? status, int page)
        {
            var url = $"comments?page={page}";
            if (!string.IsNullOrEmpty(status))
                url += "&status=" + Uri.EscapeDataString(status);
            return await _httpClient.GetFromJsonAsync<CommentPageDto>(url) ?? new();
        }

        public async Task<string?> ApproveCommentAsync(int id)
            => await (await _httpClient.PostAsync($"comments/{id}/approve", null)).GetErrorMessageAsync();

        public async Task<string?> RejectCommentAsync(int id)
            => await (await _httpClient.PostAsync($"comments/{id}/reject", null)).GetErrorMessageAsync();

        public async Task<string?> DeleteCommentAsync(int id)
            => await (await _httpClient.DeleteAsync($"comments/{id}")).GetErrorMessageAsync();
    }
}
