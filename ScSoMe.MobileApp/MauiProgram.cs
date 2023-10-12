using Blazor.Analytics;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebView.Maui;
using MudBlazor.Services;
using ScSoMe.Common;
using ScSoMe.RazorLibrary;
using Ljbc1994.Blazor.IntersectionObserver;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Auth;
using ScSoMe.RazorLibrary.Pages.Helpers;
using Microsoft.Extensions.DependencyInjection;
#if IOS
using Plugin.Firebase.iOS;
#else
using Plugin.Firebase.Android;
#endif
using Plugin.Firebase.Shared;

namespace ScSoMe.MobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .ConfigureEssentials(essentials =>
            {
                essentials.UseVersionTracking();
            })
            .ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android => android.OnCreate((activity, bundle) => MakeStatusBarTranslucent(activity)));

                static void MakeStatusBarTranslucent(Android.App.Activity activity)
                {
                    activity.Window.SetFlags(Android.Views.WindowManagerFlags.LayoutNoLimits, Android.Views.WindowManagerFlags.LayoutNoLimits);

                    activity.Window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);

                    activity.Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                }
#endif
            });

        builder.RegisterFirebaseServices();

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
        builder.Services.AddScoped<ApiClientFactory>();
        builder.Services.AddGoogleAnalytics("UA-117081036-1");
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddScoped<AppState>();
        builder.Services.AddMudServices();
        builder.Services.AddBlazorWebView();
        builder.Services.AddIntersectionObserver();
        builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddHttpClient();
        return builder.Build();
    }


    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.FinishedLaunching((app, launchOptions) => {
                CrossFirebase.Initialize(app, launchOptions, CreateCrossFirebaseSettings());
                return false;
            }));
#else
            events.AddAndroid(android => android.OnCreate((activity, state) =>
                CrossFirebase.Initialize(activity, state, CreateCrossFirebaseSettings())));
#endif
        });

        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        return builder;
    }

    private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    {
        return new CrossFirebaseSettings(isAuthEnabled: true, isCloudMessagingEnabled: true);
    }
}

