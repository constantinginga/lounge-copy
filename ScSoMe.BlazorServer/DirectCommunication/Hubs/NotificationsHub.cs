using Microsoft.AspNetCore.SignalR;
using ScSoMe.BlazorServer.DirectCommunication.Util;
using ScSoMe.RazorLibrary.Pages.ChatFeature;

namespace ScSoMe.BlazorServer.DirectCommunication.Hubs
{
    public class NotificationsHub : Hub
    {
        public readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();

        public override async Task OnConnectedAsync()
        {
            _connections.Add(NewChatMessagesNavButton.CurrentUser.Name, Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        public void NotifyOutstandingMessage(string userName, string senderName, int notification)
        {
            foreach (var connectionId in _connections.GetConnections(userName))
            {
                Clients.Client(connectionId).SendAsync("Notify", senderName, notification);
            }
        }
    }

}
