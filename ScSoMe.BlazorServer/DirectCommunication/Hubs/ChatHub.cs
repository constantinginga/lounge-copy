
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using ScSoMe.BlazorServer.DirectCommunication.Util;
using ScSoMe.RazorLibrary.Pages;
using ScSoMe.RazorLibrary.Pages.Helpers;

namespace ScSoMe.BlazorServer.DirectCommunication.Hubs
{
    
    public class ChatHub : Hub
    {
        public readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();

        public override async Task OnConnectedAsync()
        {         
            _connections.Add(Chat.CurrentUser.Name, Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        public void SendGroupInvitationToUser(string receiver, MessageSignalR messageAndSender, string groupToJoin)
        {
            foreach (var connectionId in _connections.GetConnections(receiver))
            {
                Clients.Client(connectionId).SendAsync("ReceiveInvitation", receiver, messageAndSender, groupToJoin);
            }
        }

        public void MoveChatOnTop(string receiver,string chatGroup, MessageSignalR message)
        {
            foreach (var connectionId in _connections.GetConnections(receiver))
            {
                Clients.Client(connectionId).SendAsync("MoveGroupOnTop", receiver,chatGroup, message);
            }
        }
        public void SendChatMessageToGroup(string groupName, MessageSignalR message)
        {
            Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }

        public Task JoinGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public Task LeaveGroup(string groupName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }


    }
}
