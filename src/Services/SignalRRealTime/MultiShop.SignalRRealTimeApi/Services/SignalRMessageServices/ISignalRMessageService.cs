namespace MultiShop.SignalRRealTimeApi.Services.SignalRMessageServices
{
    public interface ISignalRMessageService
    {
        Task<int> GetTotalMessageCountByRecieverId(string id);
    }
}
