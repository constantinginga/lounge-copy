using System;
namespace ScSoMe.RazorLibrary.Pages.Helpers
{

	public interface IEventService
	{
		event Action<API.Post> OnEvent;
        event Action<List<API.Post>> OnPostsEvent;
        void SendEvent(API.Post post);
		void ClearEvents();
		void SendPostsEvent(List<API.Post> posts);
		void ClearPostsEvent();
    }

	public class EventService : IEventService
	{

		public event Action<API.Post> OnEvent;

		public event Action<List<API.Post>> OnPostsEvent;

		public void SendEvent(API.Post post)
		{
			OnEvent?.Invoke(post);
		}

		public void SendPostsEvent(List<API.Post> posts)
		{
			OnPostsEvent?.Invoke(posts);
		}

		public void ClearPostsEvent()
		{
			OnPostsEvent?.Invoke(null);
		}

		public void ClearEvents()
		{
			OnEvent?.Invoke(null);
		}
	}
}

