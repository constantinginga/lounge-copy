using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace ScSoMe.RazorLibrary.Pages.Components
{
    public partial class Post
    {
        [EditorRequired]
        [Parameter]
        public int GroupId { get; set; }

        [EditorRequired]
        [Parameter]
        public long PostId { get; set; }
        [Parameter]
        public EventCallback<long> OnDelete { get; set; }

        public API.ScSoMeApi? client { get; set; }

        [Parameter]
        public API.Post? post { get; set; }

        public API.Comment? PostToComment { get; set; }

        public API.MemberInfo PostAuthor { get; set; }

        [Parameter]
        public bool IsCollapsed { get; set; } = true;

        public int TotalItems { get; set; } = 0;

        public int ReplyCount { get; set; } = 0;

        public ICollection<API.Comment> AllComments { get; set; }


        // runs after the parameters are changed
        protected override async Task OnParametersSetAsync()
        {
            if (!AppState.IsLoggedIn)
            {
                return;
            }

            try
            {
                if (post != null && post.Id == PostId)
                {
                    // reuse provided post
                }
                else
                {
                    post = await client.GetPostWithCommentsAsync(PostId);
                }
            }
            catch (Exception)
            {
                return;
            }

            if (post != null)
            {
                PostToComment = new API.Comment
                {
                    Id = post.Id,
                    CreatedDt = post.CreatedDt,
                    UpdatedDt = post.UpdatedDt,
                    AuthorMemberId = post.AuthorMemberId,
                    Text = post.Text,
                    BrowserLikeType = post.BrowserLikeType,
                    LikeType2Count = post.LikeType2Count,
                    Responses = post.Responses,
                    HasMedia = post.HasMedia
                };

                ReplyCount = post.Responses.Count();
                foreach (var one in post.Responses)
                {
                    ReplyCount += one.Responses.Count();

                    foreach (var two in one.Responses)
                    {
                        ReplyCount += two.Responses.Count();
                    }
                }

                //PostAuthor = await client.GetMemberInfoByIdAsync(PostToComment.AuthorMemberId);
            }
            await InvokeAsync(StateHasChanged);
        }

        // runs only once
        protected override async Task OnInitializedAsync()
        {
            client = apiClientFactory.GetApiClient();                        
        }

        private async Task DeleteComment((long messageId, int level) t)
        {
            switch (t.level)
            {
                case -1:
                    if (PostToComment.Id == t.messageId)
                    {
                        PostToComment = null;
                    }
                    break;
                case 0:
                    foreach (var f in PostToComment.Responses.ToList())
                    {
                        if (f.Id == t.messageId)
                        {
                            PostToComment.Responses.Remove(f);
                            ReplyCount--;
                            if (f.Responses != null)
                            {
                                ReplyCount -= f.Responses.Count();
                                foreach (var s in f.Responses)
                                {
                                    if (s.Responses != null)
                                    {
                                        ReplyCount -= s.Responses.Count();
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 1:
                    foreach (var f in PostToComment.Responses)
                    {
                        foreach (var s in f.Responses)
                        {
                            if (s.Id == t.messageId)
                            {
                                f.Responses.Remove(s);
                                ReplyCount--;
                                if (s.Responses != null)
                                {
                                    ReplyCount -= s.Responses.Count();
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    foreach (var f in PostToComment.Responses)
                    {
                        foreach (var s in f.Responses)
                        {
                            foreach (var th in s.Responses)
                            {
                                if (th.Id == t.messageId)
                                {
                                    s.Responses.Remove(th);
                                    ReplyCount--;
                                }
                            }
                        }
                    }
                    break;
            }

            await InvokeAsync(StateHasChanged);
            await OnDelete.InvokeAsync(t.messageId);
        }

        void CommentHandler((string msg, long parentId, int lvl) tuple)
        {
            API.Comment newComment = JsonConvert.DeserializeObject<API.Comment>(tuple.msg);
            IsCollapsed = false;
            ReplyCount++;
            //Console.WriteLine("FROM POST: " + tuple.msg);
            //Console.WriteLine("LEVEL: " + tuple.lvl);
            if (tuple.lvl == -1 && PostToComment.Id == tuple.parentId)
            {
                if (PostToComment.Responses == null) PostToComment.Responses = new Collection<API.Comment>();
                var PostList = PostToComment.Responses.ToList();
                PostList.Insert(0, newComment);
                PostToComment.Responses = PostList;
                //Console.WriteLine("comment added L0");
            }
            else
            {
                if (PostToComment.Responses != null)
                {
                    foreach (var firstLevel in PostToComment.Responses)
                    {
                        if (tuple.lvl == 0 && firstLevel.Id == tuple.parentId)
                        {
                            if (firstLevel.Responses == null) firstLevel.Responses = new Collection<API.Comment>();
                            var firstLevelList = firstLevel.Responses.ToList();
                            firstLevelList.Insert(0, newComment);
                            firstLevel.Responses = firstLevelList;
                            //Console.WriteLine("comment added L1");
                        }
                        else
                        {
                            if (firstLevel.Responses != null)
                            {
                                foreach (var secLevel in firstLevel.Responses)
                                {
                                    if (tuple.lvl == 1)
                                    {
                                        if (secLevel.Responses == null) secLevel.Responses = new Collection<API.Comment>();
                                        var secLevelList = secLevel.Responses.ToList();
                                        secLevelList.Insert(0, newComment);
                                        secLevel.Responses = secLevelList;
                                        //Console.WriteLine("comment added L2");
                                    }
                                    else
                                    {
                                        //
                                    }
                                }
                            }
                        }
                    }
                }
            }
            InvokeAsync(StateHasChanged);
        }


    }
}
