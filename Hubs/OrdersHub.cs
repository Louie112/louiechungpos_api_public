using Microsoft.AspNetCore.SignalR;

namespace louiechungpos.Hubs
{
    public class OrdersHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
