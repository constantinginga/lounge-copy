using System;
using Firebase.CloudMessaging;
using ObjCRuntime;

namespace ScSoMe.MobileApp.Platforms.iOS
{
	public class MessagingDelegate : IMessagingDelegate
	{
		public MessagingDelegate()
		{
            // Messaging.SharedInstance.Delegate = this;
        }

        public NativeHandle Handle => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

