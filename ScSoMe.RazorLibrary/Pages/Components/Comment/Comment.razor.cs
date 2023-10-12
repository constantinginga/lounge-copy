using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;
using System.Text.RegularExpressions;
using PSC.RazorLibrary.Components.Helpers.Quill;
using ScSoMe.RazorLibrary.Pages.Helpers;
using ScSoMe.RazorLibrary.Pages.Components.FreeUser;

namespace ScSoMe.RazorLibrary.Pages.Components.Comment
{
    public partial class Comment
    {
        [Parameter]
        public API.Comment? Message { get; set; }
        public string EmbeddedText { get; set; }
        [Parameter]
        public EventCallback<(string, long, int)> OnComment { get; set; }
        [Parameter]
        public EventCallback<(long, int)> OnDelete { get; set; }
        [Parameter]
        public long? PostId { get; set; }
        [Parameter]
        public int? GroupId { get; set; }
        [Parameter]
        public long? ParentId { get; set; }
        [Parameter]
        public int Level { get; set; }
        [Parameter]
        public bool isHidden { get; set; }
        [Inject]
        public IJSRuntime? JSRuntime { get; set; }
        public int currentMemberId { get; set; }
        public string? LastEdited;
        public QuillEditor? ReplyRef { get; set; }
        public QuillEditor? EditRef { get; set; }
        //public Color LikeColor { get; set; }
        public bool EditingEnabled { get; set; }
        public bool ReplyEnabled { get; set; }
        API.ScSoMeApi? client { get; set; }
        public API.MemberInfo? MessageAuthor { get; set; }
        public API.MemberInfo? CurrentUser { get; set; }
        [Parameter]
        public bool Collapsed { get; set; } = true;
        private string? CachedValue { get; set; }
        //tracking
        public bool IsTrackedPost { get; set; }
        //Like
        public bool IsPostLiked { get; set; }
        public API.Embedded Embedded { get; set; }
        private bool isTranslated { get; set; }
        private string? originalMsg { get; set; }
        private bool _processing { get; set; } = false;
        private string? errorMessage { get; set; }
        private string? editErrorMessage { get; set; }
        public MudTextField<string> MobileInRef { get; set; }
        public QuillEditor CommentInRef { get; set; }


        protected override async Task OnInitializedAsync()
        {
            client = apiClientFactory.GetApiClient();
        }

        protected override async Task OnParametersSetAsync()
        {
            CurrentUser = await client.GetMyMemberInfoAsync();
            MessageAuthor = await client.GetMemberInfoByIdAsync(Message.AuthorMemberId);
            currentMemberId = CurrentUser.Id;
            Message.Text = Message.Text.Replace("?<span contenteditable=\"false\">", "<span contenteditable=\"false\">");
            Message.Text = Message.Text.Replace("</span>?</span>", "</span></span>");
            CachedValue = Message.Text;
            EditingEnabled = false;
            ReplyEnabled = false;
            LastEdited = (Message.CreatedDt.CompareTo(Message.UpdatedDt) == 0) ? null : Message.UpdatedDt.Humanize();
            //check if post has already been tracked
            IsTrackedPost = await client.PostIsTrackedAsync(PostId);
            IsPostLiked = Message.BrowserLikeType.HasValue && Message.BrowserLikeType.Value == 1;
            Embedded = null;
            originalMsg = Message.Text;
            try
            {
                string embeddedStr = await client.GetEmbeddedAsync(Message.Id);
                // Console.WriteLine("STR" + embeddedStr);
                if (embeddedStr != null && !string.IsNullOrWhiteSpace(embeddedStr))
                {
                    Embedded = JsonSerializer.Deserialize<API.Embedded>(embeddedStr);
                }
            }
            catch (Exception ex)
            {
                //
            }
        }

        private async Task<API.Embedded> GetEmbedded(QuillEditor editorRef)
        {
            string embeddedStr = await editorRef.GetEmbedded();
            if (embeddedStr != null)
            {
                API.Embedded embedded = JsonSerializer.Deserialize<API.Embedded>(embeddedStr);
                if (embedded.Url != null && embedded.Img != null && embedded.Title != null)
                {
                    return embedded;
                }
            }

            return null;
        }

        private async Task Reply()
        {
            _processing = true;
            string val = await ReplyRef.GetHTML();
            try
            {
                long id = await client.CreateCommentAsync((Level == 2) ? ParentId : Message.Id, PostId, GroupId, new API.WriteMessage() { Text = val });
                API.Comment newComment = await client.GetCommentWithoutChildrenAsync(id);
                newComment.Text = newComment.Text.Replace("?<span contenteditable=\"false\">", "<span contenteditable=\"false\">");
                newComment.Text = newComment.Text.Replace("</span>?</span>", "</span></span>");
                ReplyEnabled = false;
                Embedded = await GetEmbedded(ReplyRef);
                if (Embedded != null)
                {
                    await client.CreateEmbeddedAsync(newComment.Id, Embedded);
                    await ReplyRef.ClearEmbedded();
                }
                await OnComment.InvokeAsync((JsonSerializer.Serialize(newComment), Message.Id, (Level == 2) ? Level - 1 : Level));
                _processing = false;
                errorMessage = null;
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception e)
            {
                errorMessage = "Something went wrong, please refresh the page.";
                // errorMessage = e.Message;
            }
        }

        //private void OpenFUPopup()
        //{
        //    var options = new DialogOptions { CloseOnEscapeKey = true };
        //    DialogService.Show<FreeUserPopup>("Buy membership", options);
        //}

        private async Task DeleteMessage()
        {
            if (client != null && Message != null)
            {
                if (Message.HasMedia == true)
                {
                    await client.DeletePostAllMediaFilesAsync(Message.Id);
                }
                await client.DeleteMessageAndAllChildrenAsync(Message.Id);
                await OnDelete.InvokeAsync((Message.Id, Level));
            }
        }

        private void ReplyToComment()
        {
            ReplyEnabled = !ReplyEnabled;
        }

        private async Task EditText()
        {
            _processing = true;
            try
            {
                string newValue = await EditRef.GetHTML();
                API.Embedded newEmbedded = await GetEmbedded(EditRef);
                // Console.WriteLine("TEST: " + newEmbedded.Url);
                if (CachedValue.Equals(newValue) && newEmbedded != null && newEmbedded.Equals(Embedded))
                {
                    EditingEnabled = !EditingEnabled;
                    return;
                }
                CachedValue = newValue;
                // if the new text is empty
                if (string.IsNullOrWhiteSpace(Regex.Replace(CachedValue, "<.*?>", string.Empty)))
                {
                    EditingEnabled = !EditingEnabled;
                    return;
                }
                await client.EditTextAsync(new API.WriteMessage() { Id = Message.Id, Text = CachedValue });
                Message.Text = CachedValue;
                Message.UpdatedDt = DateTimeOffset.Now;
                LastEdited = Message.UpdatedDt.Humanize();
                Embedded = newEmbedded;
                await client.CreateEmbeddedAsync(Message.Id, Embedded ?? new API.Embedded { });
                EditingEnabled = !EditingEnabled;
                _processing = false;
                editErrorMessage = null;
                originalMsg = Message.Text;
            }
            catch (Exception e)
            {
                // editErrorMessage = "Something went wrong, please refresh the page.";
                editErrorMessage = e.Message;
            }
        }

        public async Task SetEditMode(bool arg)
        {
            EditingEnabled = arg;
            if (EditingEnabled) await TranslatePost(false);
            // delay to allow CommentEmbed component to load (to get EditRef)
            await Task.Delay(20);
            if (Embedded != null)
            {
                await EditRef.CreateEmbedded(Embedded.Url, Embedded.Img, Embedded.Title);
            }
            // have to use delay, otherwise the quill editor doesn't render sometimes and it crashes
            await Task.Delay(100);
            if (EditingEnabled) await EditRef.SetSelection(Message.Text.Length, 0);
        }

        //Tracking
        private async Task TrackPost(bool toggled)
        {
            if (IsTrackedPost)
            {
                if (Message != null)
                {
                    await client.UnTrackMsgAsync(new API.TrackCommand() { PostId = PostId.Value });
                }
                IsTrackedPost = false;
            }
            else
            {
                if (Message != null)
                {
                    await client.TrackMsgAsync(new API.TrackCommand() { PostId = PostId.Value });
                }
                IsTrackedPost = true;
            }
        }

        private async Task LikePost(bool toggled)
        {
            bool result = toggled;
            if (Message != null && client != null)
            {
                result = await client.LikeMsgAsync(new API.LikeCommand() { MessageId = Message.Id, LikeType = 1 });
            }
            if (result && !IsTrackedPost)
            {
                await client.TrackMsgAsync(new API.TrackCommand() { PostId = Message.Id });
                IsTrackedPost = true;
            }

            IsPostLiked = result;
            StateHasChanged();
        }

        void OnEditingEnabledChanged(bool arg)
        {
            EditingEnabled = arg;
        }

        async Task TranslatePost(bool toggled)
        {
            isTranslated = !toggled;
            if (!isTranslated)
            {
                if (client != null && Message != null)
                {
                    long commentId = Message.Id;
                    var result = await client.TranslatePostAsync(commentId);
                    if (result != null)
                    {
                        Message.Text = result.TranslatedText;
                        isTranslated = true;

                        await InvokeAsync(StateHasChanged);
                    }
                    Console.WriteLine(Message.Text);
                    isTranslated = true;
                }
            }
            else
            {
                Message.Text = originalMsg;
                isTranslated = false;
            }
        }
    }
}

