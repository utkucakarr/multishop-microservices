using MultiShop.DtoLayer.CommentDtos;

namespace MultiShop.WebUI.Services.CommentServices
{
    /// <summary>Comment servisinin istemcisi: ürün sayfası yorumları ve admin moderasyonu.</summary>
    public interface ICommentService
    {
        /// <summary>Ürünün onaylı yorumları (en yeni önce).</summary>
        Task<List<ResultProductCommentDto>> CommentListByProductId(string productId);

        /// <summary>Yeni yorum (onay bekler). Başarıda null, değilse Comment'in mesajı.</summary>
        Task<string?> CreateCommentAsync(CreateCommentDto createCommentDto);

        /// <summary>Admin listesi; <paramref name="status"/> verilirse yalnızca o durumdakiler.</summary>
        Task<CommentPageDto> GetCommentsAsync(string? status, int page);

        /// <summary>Başarıda null, değilse Comment'in mesajı.</summary>
        Task<string?> ApproveCommentAsync(int id);

        /// <summary>Başarıda null, değilse Comment'in mesajı.</summary>
        Task<string?> RejectCommentAsync(int id);

        /// <summary>Başarıda null, değilse Comment'in mesajı.</summary>
        Task<string?> DeleteCommentAsync(int id);
    }
}
