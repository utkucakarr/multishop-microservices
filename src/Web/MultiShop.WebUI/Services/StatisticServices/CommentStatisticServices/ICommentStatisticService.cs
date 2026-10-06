namespace MultiShop.WebUI.Services.StatisticServices.CommentStatisticServices
{
    public interface ICommentStatisticService
    {
        Task<int> GetTotalCommentCountAsync();

        Task<int> GetActiveTotalCommentCountAsync();

        Task<int> GetPassiveTotalCommentCountAsync();
    }
}
