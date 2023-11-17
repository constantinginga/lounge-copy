using API;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MudBlazor;
using ScSoMe.RazorLibrary.Pages.Components.AttachFiles;
using ScSoMe.RazorLibrary.Pages.Helpers;
using ScSoMe.RazorLibrary.Pages.Components.FreeUser;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ScSoMe.RazorLibrary.Pages.Components
{
    public partial class NewPost
    {
        [Parameter]
        public DateTimeOffset DateCreated { get; set; }
        [Parameter]
        public EventCallback<API.Post> OnCreate { get; set; }
        [Parameter]
        public EventCallback OnDelete { get; set; }
        [Parameter]
        public int? GroupId { get; set; }
        public QuillEditor? InRef { get; set; }
        public API.ScSoMeApi client { get; set; }
        public API.MemberInfo CurrentUser { get; set; }
        public ICollection<API.ScGroup> groups { get; set; }
        public bool postPrivacySetting { get; set; } = false;
        private string? groupValue { get; set; }
        private bool _processing { get; set; } = false;
        private string errorMessage { get; set; } = null;
        public API.Embedded? Embedded { get; set; }
        public AttachFile attachFile { get; set; }


        protected override Task OnParametersSetAsync()
        {
            if (GroupId == null)
            {
                groupValue = groups.First().GroupName;
            }
            else
            {
                foreach (var g in groups)
                {
                    if (g.GroupId == GroupId)
                    {
                        groupValue = g.GroupName;
                        break;
                    }
                }
            }

            return Task.CompletedTask;
        }

        protected override async Task OnInitializedAsync()
        {
            if (!AppState.IsLoggedIn)
            {
                NavManager.NavigateTo("/login");
                return;
            }

            client = apiClientFactory.GetApiClient();
            groups = AppState.AllGroups;
            CurrentUser = await client.GetMyMemberInfoAsync();
            base.InRef = InRef;
            base.attachFile = attachFile;
        }

        private async Task CreatePost()
        {
            _processing = true;
            API.ScGroup? selectedGroup = groups.FirstOrDefault(g => g.GroupName == groupValue);
            if (selectedGroup != null)
            {
                if (InRef != null)
                {
                    Console.WriteLine("NEWPOST 112: " + selectedGroup.GroupId);
                    var newPost = await base.CreatePost(selectedGroup.GroupId, postPrivacySetting);
                    await OnCreate.InvokeAsync(newPost);
                    errorMessage = null;
                    _processing = false;
                }
            }
        }

        private async Task DeletePost()
        {
            await OnDelete.InvokeAsync();
        }

        protected override void OnInitialized()
        {
            AppState.OnAttachedMediaChanged += StateHasChanged;
        }


        public void Dispose()
        {
            AppState.OnAttachedMediaChanged -= StateHasChanged;
        }
    }
}
