using API;
using Microsoft.AspNetCore.SignalR.Client;
using Blazorise.Extensions;
using ScSoMe.RazorLibrary.Pages.Helpers;
using MudBlazor;
using Microsoft.JSInterop;

using System.Data;


namespace ScSoMe.RazorLibrary.Pages
{
    public partial class Chat
    {


        private WindowDimension? wd { get; set; }
        private bool openedMessagingSection = false;

        private HubConnection? hubConnection;
        private HubConnection? hubConnectionNotify;



        private string? search;

        private bool _processing { get; set; } = true;
        private bool _areWeSearching = true;
        private bool _weChattingBoys = true;
        private bool _isChattingAllowed = true;
        private bool _areWeGrouping = false;
        private bool _weGroupingBoys = true;
        private bool _deletePopover = false;
        private bool _deleteSingleMessage = false;
        private bool _isTheChatDeleted = false;
        private bool _isTheChatDeletedPopup = false;
        private bool _weEditingBoys = false;
        private bool unreadMessages = false;
        private bool autofocus = false;

        private int _count;

        private string? receiverAvatar;
        private string? receiverUsername;
        private string? newGroupName;
        private string? displayedGroupName;
        private string? groupNameInternal;
        private string? _lastMessage;
        private int inputLines = 2;

        

        private DialogOptions dialogOptions = new() { FullWidth = true };
        private DialogOptions dialogEditOptions = new() { FullWidth = true };
        private DialogOptions dialogOptionsChatDeleted = new() { FullWidth = true };
        private DialogOptions dialogSingleMessageOptions = new() { FullWidth = true };

        private MudTextField<string>? MessageRef { get; set; }
        private MudTextField<string>? MessageRefSearch { get; set; }

        private List<MessageSignalR> sentMessages = new();
        private List<string>? searchGroup = new List<string>();
        private List<ChatMember> allGroupsUserIsIn = new();
        private List<MessageHistory> messageHistory = new();

        private List<ChatMember> chatMembersInvited = new List<ChatMember>();
   
        private API.ScSoMeApi? client { get; set; }

        public static API.MemberInfo? CurrentUser { get; set; }
        public static int CurrentUserId { get; set; }

        
        public class MessageInput
        {
            public string? Message { get; set; }
        }
        
        MessageInput messageInput = new();

        protected override async Task OnInitializedAsync()
        {
            wd = await JSRuntime.InvokeAsync<WindowDimension>("getWindowDimensions");
            if (wd.Width > 500)
            {
                inputLines = 1;
                autofocus = true;
            }
            if (!AppState.IsLoggedIn)
            {
                NavManager.NavigateTo("./login");
                return;
            }
            _areWeSearching = true;
           
            client = ApiClientFactory.GetApiClient();
            CurrentUser = AppState.CurrentUser;
            CurrentUserId = CurrentUser.Id;

            allGroupsUserIsIn = await GetChats();
            if (allGroupsUserIsIn != null)
            {
                _processing = false;
            }
            
             hubConnection = new HubConnectionBuilder()
            .WithUrl(NavManager.ToAbsoluteUri("https://startupcentral.dk/lounge/chathub"))
            .Build();

            await hubConnection.StartAsync();
            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                hubConnection = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("https://groups.startupcentral.dk/chathub"))
                .Build();
                await hubConnection.StartAsync();
            }

            hubConnectionNotify = new HubConnectionBuilder()
            .WithUrl(NavManager.ToAbsoluteUri("https://startupcentral.dk/lounge/notificationshub"))
            .Build();

            await hubConnectionNotify.StartAsync();
            if (hubConnectionNotify.State == HubConnectionState.Disconnected)
            {
                hubConnectionNotify = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("https://groups.startupcentral.dk/notificationshub"))
                .Build();
                await hubConnectionNotify.StartAsync();
            }

            hubConnection.On<MessageSignalR>("ReceiveMessage", (message) =>
            {
                sentMessages.Insert(0,message);
                StateHasChanged();
            });

            hubConnection.On<string,string, MessageSignalR>("MoveGroupOnTop",async (receiver,chatGroup,message) =>
            {
                var usersInChat = chatGroup.Split(',');
                var chatGroupInversed = $"{usersInChat[1]},{usersInChat[0]}";
                for (int i = 0; i < allGroupsUserIsIn.Count; i++)
                {
                    if(allGroupsUserIsIn[i].groupUserIsIn.Equals(chatGroup) || allGroupsUserIsIn[i].groupUserIsIn.Equals(chatGroupInversed))
                    {
                        var temp = allGroupsUserIsIn[i];
                        temp.latestGroupMessageInfo = await client.GetLastChatMessageAsync(chatGroup);
                        temp.unreadMessages = await client.GetTotalUnreadMessageCountAsync(AppState.CurrentUser.Id);
                        allGroupsUserIsIn.RemoveAt(i);
                        allGroupsUserIsIn.Insert(0, temp);
                    }
                }
                for (int i = 0; i < chatMembersInvited.Count; i++)
                {
                    if (chatMembersInvited[i].groupUserIsIn.Equals(chatGroup) || chatMembersInvited[i].groupUserIsIn.Equals(chatGroupInversed))
                    {
                        var temp = chatMembersInvited[i];
                        temp.latestGroupMessageInfo = await client.GetLastChatMessageAsync(chatGroup);
                        temp.unreadMessages = await client.GetTotalUnreadMessageCountAsync(AppState.CurrentUser.Id);
                        chatMembersInvited.RemoveAt(i);
                        chatMembersInvited.Insert(0, temp);
                    }
                }
                StateHasChanged();
            });

            hubConnection.On<string, MessageSignalR, string>("ReceiveInvitation", async (receiver, messageAndSender, groupToJoin) =>
            {
                var messageToHistory = new MessageHistory();
                messageToHistory.Message = messageAndSender.message;
                messageToHistory.SentDate = DateTime.Now;

                //for checking if user toBeInvited already was invited
                ChatMember toBeInvited = new ChatMember(groupToJoin, messageAndSender.senderName, messageAndSender.avatar, $"{receiver},{messageAndSender.senderName}", messageToHistory, null, 0);
                
                // checking if a user exists in a chat
                if (allGroupsUserIsIn.Contains(toBeInvited) || chatMembersInvited.Contains(toBeInvited)) { 
                    return;
                }
                else
                {
                    toBeInvited.unreadMessages = 1;
                    chatMembersInvited.Insert(0,toBeInvited);
                }
                StateHasChanged();
            });
            //await hubConnection.StartAsync();
            //await hubConnectionNotify.StartAsync();
        }

        public async Task<List<MessageHistory>> GetMessageHistory(string groupName)
        {
            var history = await client!.GetChatMessageHistoryAsync(groupName);
            messageHistory = new List<MessageHistory>();
            foreach (var msg in history)
            {
                messageHistory.Add(msg);
            }
            return messageHistory;
        }

        public async Task<List<ChatMember>> GetChats()
        {
            chatMembersInvited.Clear();
            CurrentUser = AppState.CurrentUser;
            var CurrentUserIdString = CurrentUser!.Id.ToString();
            var groupUsers = new List<ChatMember>();
            var chatResult = await client!.GetAllChatsByUserAsync(CurrentUser.Id);

            foreach (var chatRes in chatResult)
            {
                var groupUser = new ChatMember();
                var member = new MemberInfo();

                groupUser.groupUserIsIn = chatRes.GroupName;
                groupUser.groupDisplayName = chatRes.DisplayName;
                groupUser.newDisplayName = chatRes.NewDisplayName;
                
                string[] split = chatRes.GroupName.Split(',');


                if (split[0] == CurrentUserIdString)
                {
                    member = await client.GetMemberInfoByIdAsync(Int32.Parse(split[1]));
                }
                else
                {
                    member = await client.GetMemberInfoByIdAsync(Int32.Parse(split[0]));
                }
                groupUser.memberName = member.Name;
                groupUser.avatar = member.Avatar;
                groupUser.unreadMessages = await client.GetUnreadMessageCountAsync(groupUser.groupUserIsIn, CurrentUser.Id);

                // Arranging group list in order of the latest message received  
                var messageInfo = await client.GetLastChatMessageAsync(groupUser.groupUserIsIn);

                if (messageInfo != null)
                {
                    groupUser.latestGroupMessageInfo = messageInfo;
                }
                groupUsers.Insert(0,groupUser); 
            }
            return groupUsers.OrderByDescending(o => o.latestGroupMessageInfo.SentDate).ToList();
        }

        public async Task OpenSavedChat(string chatGroupName, string displayName, string receiverAvatar, string receiverName, string newDisplayName)
        {
            
            openedMessagingSection = true;
            _areWeSearching = false;
            CurrentUser = AppState.CurrentUser;
            if (groupNameInternal is not null)
            {
                await hubConnection!.SendAsync("LeaveGroup", groupNameInternal);
            }
            sentMessages.Clear();
            if(!newDisplayName.IsNullOrEmpty())
            {
                displayedGroupName = newDisplayName;
            }
            else
            {
                displayedGroupName = displayName;
            }
            groupNameInternal = chatGroupName;
            receiverUsername = receiverName;
            this.receiverAvatar = receiverAvatar;
            if(!await client!.CheckIfDeletedAsync(chatGroupName))
            {
                await client.MarkMessagesAsReadAsync(chatGroupName, CurrentUser.Id);
                await hubConnection!.SendAsync("JoinGroup", chatGroupName);
                
                _areWeGrouping = false;
                await GetMessageHistory(chatGroupName);
                _isTheChatDeletedPopup = false;
                messageInput = new();
            }
            else
            {
                _isTheChatDeletedPopup = true;
            }

            var hasNewMessage = await client.GetTotalUnreadMessageCountAsync(CurrentUser!.Id);
            
            await hubConnectionNotify!.SendAsync("NotifyOutstandingMessage", CurrentUser.Name, CurrentUser.Name, hasNewMessage);
            //allGroupsUserIsIn = await GetChats();
            
            //---- Verifying if user has unread messages
            for (int i = 0; i < allGroupsUserIsIn.Count; i++)
            {
                if (allGroupsUserIsIn[i].groupUserIsIn.Equals(chatGroupName))
                {
                    var temp = allGroupsUserIsIn[i];                    
                    temp.unreadMessages = 0;
                    allGroupsUserIsIn.RemoveAt(i);
                    allGroupsUserIsIn.Insert(i, temp);
                }
            }
            //----

            //---- Verifying if user has unread messages in the fresh started chats 
            for (int i = 0; i < chatMembersInvited.Count; i++)
            {
                if (chatMembersInvited[i].groupUserIsIn.Equals(chatGroupName))
                {
                    var temp = chatMembersInvited[i];
                    temp.unreadMessages = 0;
                    chatMembersInvited.RemoveAt(i);
                    chatMembersInvited.Insert(i, temp);
                }
            }
            //----
            await InvokeAsync(StateHasChanged);
        }
        
        private async Task SendMessageButtonAction()
        {
            CurrentUser = AppState.CurrentUser;
            SendMessage(CurrentUser!.Name, CurrentUser.Id, groupNameInternal!, messageInput.Message);
            messageInput = new();
            await InvokeAsync(StateHasChanged);
        }

        private async void SendMessage(string senderUsername, int senderID, string chatGroupName, string message)
        {

            if (!message.IsNullOrEmpty() && !message!.All(char.IsWhiteSpace))
            {
                MessageSignalR messageObj = new MessageSignalR();
                messageObj.message = message!;
                messageObj.senderId = senderID;
                messageObj.senderName = senderUsername;
                messageObj.isRead = "false";
                messageObj.sentDate = DateTime.Now;


                _isTheChatDeleted = await client!.CheckIfDeletedAsync(chatGroupName);
                if (!_isTheChatDeleted)
                {
                    await hubConnection!.SendAsync("SendChatMessageToGroup", chatGroupName, messageObj);
                    await client.MarkMessagesAsReadAsync(chatGroupName, CurrentUser!.Id);

                    await client.SaveMessageAsync(chatGroupName, message, senderID, senderUsername);

                    var usersInChat = chatGroupName.Split(',');

                    var temp = await client.GetMemberInfoByIdAsync(Int32.Parse(usersInChat[0]));

                    var temp1 = await client.GetMemberInfoByIdAsync(Int32.Parse(usersInChat[1]));


                    if (temp.Id == CurrentUser.Id)
                    {
                        var hasNewMessage = await client.GetTotalUnreadMessageCountAsync(temp1.Id);
                        await hubConnectionNotify!.SendAsync("NotifyOutstandingMessage", temp1.Name, CurrentUser.Name, hasNewMessage);
                    }
                    else
                    {
                        var hasNewMessage = await client.GetTotalUnreadMessageCountAsync(temp.Id);
                        await hubConnectionNotify!.SendAsync("NotifyOutstandingMessage", temp.Name, CurrentUser.Name, hasNewMessage);
                    }

                    // Arranging group list in order of the latest message received 
                    await hubConnection!.SendAsync("MoveChatOnTop", temp.Name, chatGroupName, messageObj);
                    await hubConnection!.SendAsync("MoveChatOnTop", temp1.Name, chatGroupName, messageObj);

                    // Change notification dot when sending a message
                    //var hasNewMessage = await client.GetTotalUnreadMessageCountAsync(CurrentUser!.Id);
                    //var hasNewMessage = await client.GetTotalUnreadMessageCountAsync(temp.Id);
                    // cia yra problema notifications rodo kai siunti is vieno i kita, todel reik suzaisti su kokiu if, kad darytu normal

                    //await hubConnectionNotify!.SendAsync("NotifyOutstandingMessage", CurrentUser.Name, CurrentUser.Name, hasNewMessage);
                }
                else
                {
                    _isTheChatDeletedPopup = true;
                }
            }


        }

        public bool IsConnected => hubConnection?.State == HubConnectionState.Connected;

        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }

        private async Task<DcHistory> JoinGroup(string senderUserName, string receiverUserName, int senderUserID, int recieverUserID)
        {
            var groupName = senderUserID + "," + recieverUserID;
            var displayName = senderUserName + "," + receiverUserName;
            var chatReturn = await client!.CheckChatExistAsync(senderUserID, recieverUserID, groupName, displayName);
            displayedGroupName = chatReturn.DisplayName;
            if (hubConnection is not null) {
                await hubConnection.SendAsync("JoinGroup", chatReturn.GroupName);
            }

            return chatReturn;
        }


        private void WeSearchingBoys()
        {
            _areWeSearching = true;
            openedMessagingSection = true;
        }

        public void WeGroupingBoys()
        {
            _areWeSearching = false;
            _areWeGrouping = true;
        }

        private async Task<IEnumerable<string>> SearchForUsers(string value)
        {
            Stack<string> userNames = new Stack<string>();

            if (string.IsNullOrEmpty(value))
                return null!;
            var current = AppState.CurrentUser;
            var response = await client!.SearchMembersAsync(value);
            foreach (var users in response)
            {
                if(users.Id != current!.Id)
                {
                    userNames.Push(users.Name);
                }
            }
            _weChattingBoys = userNames.IsNullOrEmpty();
            return userNames.Distinct();
        }

        private async Task AddToGroup()
        {
            searchGroup!.Add(search!);
            search = "";
            if (searchGroup.Count() >= 2)
            {
                _weGroupingBoys = false;
            }
        }


        private async Task SendNewMessageAsync()
        {
            _processing = true;
            CurrentUser = AppState.CurrentUser;

            var userToList = await client!.SearchMembersAsync(search);
            var memberFound = userToList.FirstOrDefault();

            string groupToJoinName = CurrentUser!.Id + "," + memberFound.Id;

            var chatReturn = await JoinGroup(CurrentUser.Name!, memberFound.Name, CurrentUser.Id, memberFound.Id);

            SendMessage(CurrentUser.Name, CurrentUser.Id, chatReturn.GroupName, messageInput.Message!);

            MessageSignalR msg = new();
            msg.message = messageInput.Message!;
            msg.senderId = CurrentUser.Id;
            msg.senderName = CurrentUser.Name;
            msg.isRead = "false";
            msg.sentDate = DateTime.Now;
            msg.avatar = CurrentUser.Avatar;

            Task t1 = hubConnection!.SendAsync("SendGroupInvitationToUser", memberFound.Name, msg, chatReturn.GroupName);

            search = "";
            var receiverA = await client.GetMemberInfoByIdAsync(memberFound.Id);
            receiverAvatar = receiverA.Avatar;
            receiverUsername = receiverA.Name;

            // open new chat window or open the existing one
            Task t2 =  OpenSavedChat(chatReturn.GroupName, chatReturn.DisplayName, receiverAvatar, receiverUsername,null);

            // messageHistory.RemoveAt(messageHistory.Count - 1);

            ////Task t3 = GetChats();
            allGroupsUserIsIn = await GetChats();
            
            await Task.WhenAll(t1,t2);
            //chatMembersInvited.Clear();
            messageInput = new();

            await Task.Delay(450);
            sentMessages.Clear();
            _weChattingBoys = true;
            _isChattingAllowed = true;
            _processing = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task<string> LastMessage(string groupName)
        {
            var _lastMessageHistory = new MessageHistory();
            _lastMessageHistory = await client!.GetLastChatMessageAsync(groupName);
            if(_lastMessageHistory != null)
            {
                _lastMessage = _lastMessageHistory.Message;
                return _lastMessage;
            }
            else
            {
                throw new Exception("There were no previous messages in the chat: " + groupName);
            }
        }

        private async Task DeleteSingleRMessage(MessageSignalR? message)
        {
            var messageToDelete = sentMessages.Single(x => x.message == message!.message && x.sentDate == message.sentDate);
            sentMessages.Remove(messageToDelete);

            await client!.DeleteSingleMessageAsync(groupNameInternal, message!.message, message.senderId, message.sentDate);
            _deleteSingleMessage = false;

        }

        private async Task DeleteSingleDbMessage(MessageHistory message)
        {
            await client!.DeleteSingleMessageAsync(groupNameInternal, message.Message, message.SenderId, message.SentDate);
            messageHistory = await GetMessageHistory(groupNameInternal!);
            sentMessages.Clear();
            _deleteSingleMessage = false;
        }

        private async Task DeleteChat(string groupName)
        {
            var groupToRemove = allGroupsUserIsIn.FirstOrDefault(x => x.groupUserIsIn == groupName);
            if(groupToRemove != null)
            {
                allGroupsUserIsIn.Remove(groupToRemove);
            }
            var groupToRemoveSingalR = chatMembersInvited.FirstOrDefault(c => c.groupUserIsIn== groupName);
            if(groupToRemoveSingalR != null)
            {
               chatMembersInvited.Remove(groupToRemoveSingalR);
            }

            await client!.DeleteChatAsync(groupName);

            displayedGroupName = "";
            messageHistory.Clear();
            sentMessages.Clear();
            allGroupsUserIsIn = await GetChats();
            _deletePopover = false;
            _areWeSearching = true;
        }
        
        private void ToggleOpenDeleteChat()
        {
            if (_deletePopover)
                _deletePopover = false;
            else
                _deletePopover = true;
        }

        private void ToggleOpenEditChat()
        {
            if (_weEditingBoys)
                _weEditingBoys = false;
            else
                _weEditingBoys = true;
        }
        
        private void OpenDialog(string groupName)
        {
            var parameters = new DialogParameters();
            parameters.Add("groupNameDelete", groupName);
            parameters.Add("allGroupsUserIsIn2", allGroupsUserIsIn);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            DialogService.Show<DeleteMessages>("Delete", parameters, options);
        }
        
        private async Task ChangeGroupName(string newGroupName)
        {
            await client!.ChangeChatDisplayNameAsync(groupNameInternal, newGroupName);
            displayedGroupName = newGroupName;
            allGroupsUserIsIn = await GetChats();
            _weEditingBoys = false;
            this.newGroupName = "";
        }
        
        private async Task ChatWasDeleted()
        {
            var groupToRemove = allGroupsUserIsIn.FirstOrDefault(x => x.groupUserIsIn == groupNameInternal);
            if (groupToRemove != null)
            {
                allGroupsUserIsIn.Remove(groupToRemove);
            }
            var groupToRemoveSignalR = chatMembersInvited.FirstOrDefault(c => c.groupUserIsIn == groupNameInternal);
            if (groupToRemoveSignalR != null)
            {
                chatMembersInvited.Remove(groupToRemoveSignalR);
            }
            _isTheChatDeletedPopup = false;
            _areWeSearching = true;
            allGroupsUserIsIn = await GetChats();
            await InvokeAsync(StateHasChanged);
        }


    }
}