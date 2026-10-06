using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using MultiShop.SignalRRealTimeApi.Services;
using MultiShop.SignalRRealTimeApi.Services.SignalRCommentServices;
using MultiShop.SignalRRealTimeApi.Services.SignalRMessageServices;

namespace MultiShop.SignalRRealTimeApi.Hubs
{
    public class SignalRHub : Hub
    {
        private readonly ISignalRCommentServices _signalRCommentServices;
        //private readonly ISignalRMessageService _signalRMessageService;

        public SignalRHub(ISignalRCommentServices signalRCommentServices)
        {
            _signalRCommentServices = signalRCommentServices;
        }

        public async Task SendStatisticCount()
        {
            var getTotalCommentCount = await _signalRCommentServices.GetTotalCommentCountAsync();
            await Clients.All.SendAsync("RecieveCommentCount", getTotalCommentCount);
        }
    }
}
