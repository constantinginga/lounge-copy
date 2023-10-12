using Foundation;
using UIKit;
using Firebase.Core;
using UserNotifications;
using Firebase.CloudMessaging;

namespace ScSoMe.MobileApp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        return MauiProgram.CreateMauiApp();
    }

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {

        var returnVal = base.FinishedLaunching(app, options);

        try
        {
            Firebase.Core.App.Configure();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        // Register your app for remote notifications.
        if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
        {
            // For iOS 10 display notification (sent via APNS)
            //UNUserNotificationCenter.Current.Delegate = this;

            var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
            UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
            {
                Console.WriteLine(granted);
            });
        }
        else
        {
            // iOS 9 or before
            var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
            var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
        }

        UIApplication.SharedApplication.RegisterForRemoteNotifications();

        // DO THIS WHEN SENDING A NOTIFICATION
        UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;


        var md = new MessagingDelegate();
        var token = Messaging.SharedInstance.FcmToken ?? "";
        Console.WriteLine($"FCM token: {token}");
        Preferences.Set("iOSToken", token);
        return returnVal;
    }
}

