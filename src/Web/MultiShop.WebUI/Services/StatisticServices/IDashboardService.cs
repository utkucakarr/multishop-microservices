using MultiShop.DtoLayer.StatisticDtos;

namespace MultiShop.WebUI.Services.StatisticServices
{
    public interface IDashboardService
    {
        /// <summary>Admin › İstatistikler: beş servisin rakamları tek istekte (Ocelot aggregate). İstek başarısızsa tüm alanlar null.</summary>
        Task<AdminDashboardDto> GetDashboardAsync();

        /// <summary>Admin üst menüsü (her sayfada): yalnızca Comment; alınamazsa null.</summary>
        Task<CommentStatisticsDto?> GetCommentStatisticsAsync();

        /// <summary>Admin üst menüsü (her sayfada): yalnızca Message; alınamazsa null.</summary>
        Task<MessageStatisticsDto?> GetMessageStatisticsAsync();
    }
}
