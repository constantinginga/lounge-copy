using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace ScSoMe.MobileApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    internal static readonly string Channel_ID = "TestChannel";
    internal static readonly int NotificationID = 101;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        if (Intent.Extras != null)
        {
            foreach (var key in Intent.Extras.KeySet())
            {
                if (key == "NavigationID")
                {
                    string idValue = Intent.Extras.GetString(key);
                    if (Preferences.ContainsKey("NavigationID"))
                    {
                        Preferences.Remove("NavigationID");
                    }
                    Preferences.Set("NavigationID", idValue);
                }
            }
        }
        CreateNotificationChannel();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            Window.InsetsController?.SetSystemBarsAppearance((int)WindowInsetsControllerAppearance.LightStatusBars, (int)WindowInsetsControllerAppearance.LightStatusBars);
        }
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M && Build.VERSION.SdkInt < BuildVersionCodes.R)
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
        }

        
    }

    public override bool DispatchKeyEvent(KeyEvent e)
    {
        if (e.KeyCode == Keycode.Back)
        {
            if (e.Action == KeyEventActions.Down)
            {
                // handle back button
            }

            return true;
        }
        return base.DispatchKeyEvent(e);
    }

    private void CreateNotificationChannel()
    {
        if (OperatingSystem.IsOSPlatformVersionAtLeast("android", 26))
        {
            var channel = new NotificationChannel(Channel_ID, "Test Notification Channel", NotificationImportance.Default);

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}

