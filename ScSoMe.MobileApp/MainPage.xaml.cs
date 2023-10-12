using System.Text;
using Microsoft.AspNetCore.Components.WebView;
using Newtonsoft.Json;
using ScSoMe.MobileApp.Models;
//using ScSoMe.Common;

namespace ScSoMe.MobileApp;

public partial class MainPage : ContentPage
{

	private string _deviceToken;
	//private API.ScSoMeApi? client;
	//private ApiClientFactory apiClientFactory;

    public MainPage()
	{
		InitializeComponent();
		//apiClientFactory = new ApiClientFactory();
		//client = apiClientFactory.GetApiClient();

        if (Preferences.ContainsKey("DeviceToken"))
		{
			_deviceToken = Preferences.Get("DeviceToken", "");
            Console.WriteLine("DEVICETOKEN: " + _deviceToken);


        }

		if (Preferences.ContainsKey("NavigationID"))
		{

		}
	}


	private void Handle_UrlLoading(object sender, UrlLoadingEventArgs urlArgs)
	{
		var loadingUrl = urlArgs.Url.AbsolutePath.ToString().ToUpper().Replace("/", "");
		var host = urlArgs.Url.Host.ToString().ToUpper();

        // Load these URLs inside the app
        if (host.Contains(AllowedIFrames.YOUTUBE.ToString()) ||
            host.Contains(AllowedIFrames.REVOLUT.ToString()) ||
         host.Contains(AllowedIFrames.PAYLIKE.ToString()) ||
			host.Contains(AllowedIFrames.STARTUPCENTRAL.ToString())
			&& (loadingUrl.Contains(AllowedIFrames.COACHES.ToString()) || loadingUrl.Contains(AllowedIFrames.PARTNER.ToString())))
		{
			urlArgs.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView;
		}
    }

	private async void OnCounterClicked(object sender, EventArgs e)
	{
		var androidNotificationObject = new Dictionary<string, string>();
		androidNotificationObject.Add("NavigationID", "1");

		var pushNotificationRequest = new PushNotificationRequest
		{
			notification = new NotificationMessageBody
			{
				title = "Notification Title",
				body = "Notification body"
			},
			data = androidNotificationObject,
			registration_ids = new List<string> { _deviceToken }
		};

		string url = "https://fcm.googleapis.com/fcm/send";

		using (var client = new HttpClient())
		{
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("key", "=" + "AAAAOIMN97M:APA91bFkU1j2-CNsRBtMnmsOiX0bcRJZwRkZfc_Z-LzQGXrcud48JwBren5F7JF1-EY5HZQDCY2wGNZLYyfmR2Lp5w4YX_2TUaE5EjL3RdodQ9W_mbyTpv5qeH-5rTXbonLVMkwr9878");
			string serializeRequest = JsonConvert.SerializeObject(pushNotificationRequest);
			var response = await client.PostAsync(url, new StringContent(serializeRequest, Encoding.UTF8, "application/json"));
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				await App.Current.MainPage.DisplayAlert("Notification sent", "notification sent", "OK");
			}
			else
			{
				Console.WriteLine("STATUSCODE: " + response.StatusCode);
			}
		}
	}

	enum AllowedIFrames
    {
		YOUTUBE,
		STARTUPCENTRAL,
		COACHES,
		PARTNER,
		PAYLIKE,
		REVOLUT
    }
}

