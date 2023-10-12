using System;
using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Firebase.Messaging;

namespace ScSoMe.MobileApp.Platforms.Android.Services
{
	[Service(Exported = true)]
	[IntentFilter(new[] {"com.google.firebase.MESSAGING_EVENT"})]
	public class FirebaseService : FirebaseMessagingService
	{
		public FirebaseService()
		{

		}

        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
			if (Preferences.ContainsKey("DeviceToken"))
			{
				Preferences.Remove("DeviceToken");
			}
			Preferences.Set("DeviceToken", token);
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

			var notification = message.GetNotification();
			SendNotification(notification.Body, notification.Title, message.Data);
        }

		private void SendNotification(string messageBody, string title, IDictionary<string, string> data)
		{
			var intent = new Intent(this, typeof(MainActivity));
			intent.AddFlags(ActivityFlags.ClearTop);

			foreach (var key in data.Keys)
			{
				string value = data[key];
				intent.PutExtra(key, value);
			}

			var pendingIntent = PendingIntent.GetActivity(this, MainActivity.NotificationID, intent, PendingIntentFlags.Mutable);

			var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.Channel_ID)
				.SetContentTitle(title)
				.SetSmallIcon(Resource.Mipmap.greenrocket_android)
				.SetContentText(messageBody)
				.SetChannelId(MainActivity.Channel_ID)
				.SetContentIntent(pendingIntent)
				.SetPriority(2)
				.SetAutoCancel(true);

			var notificationManager = NotificationManagerCompat.From(this);
			notificationManager.Notify(MainActivity.NotificationID, notificationBuilder.Build());
		}
    }
}

