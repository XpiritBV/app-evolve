using System.Reflection;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Android.Widget;
using Android.Util;
using FormsToolkit.Droid;
using Refractored.XamForms.PullToRefresh.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Clients.UI;
using Xamarin.Forms.Platform.Android.AppLinks;
using Xamarin;
using FormsToolkit;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Droid
{
	[Activity(Label = XamarinEvolve.Utils.EventInfo.EventShortName, Icon = "@drawable/ic_launcher", LaunchMode = LaunchMode.SingleTask, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { Intent.ActionView },
		Categories = new[]
		{
            Intent.CategoryDefault,
            Intent.CategoryBrowsable
		},
		DataScheme = "http",
		DataPathPrefix = "/" + Utils.AboutThisApp.SessionsSiteSubdirectory + "/",
		DataHost = Utils.AboutThisApp.AppLinksBaseDomain)]
	[IntentFilter(new[] { Intent.ActionView },
		Categories = new[]
		{
            Intent.CategoryDefault,
            Intent.CategoryBrowsable
		},
		DataScheme = "http",
      	DataPathPrefix = "/" + Utils.AboutThisApp.SpeakersSiteSubdirectory + "/",
		DataHost = Utils.AboutThisApp.AppLinksBaseDomain)]
	[IntentFilter(new[] { Intent.ActionView },
		Categories = new[]
		{
            Intent.CategoryDefault,
            Intent.CategoryBrowsable
		},
		DataScheme = "https",
		DataPathPrefix = "/" + Utils.AboutThisApp.SpeakersSiteSubdirectory + "/",
		DataHost = Utils.AboutThisApp.AppLinksBaseDomain)]
	[IntentFilter(new []{ Intent.ActionView },
        Categories = new []
        {
            Intent.CategoryDefault,
            Intent.CategoryBrowsable
        },
        DataScheme = "http",
	    DataPathPrefix = "/" + Utils.AboutThisApp.MiniHacksSiteSubdirectory + "/",
        DataHost = Utils.AboutThisApp.AppLinksBaseDomain)]
	[IntentFilter(new[] { Intent.ActionView },
		Categories = new[]
		{
            Intent.CategoryDefault,
            Intent.CategoryBrowsable
		},
		DataScheme = "https",
		DataPathPrefix = "/" + Utils.AboutThisApp.MiniHacksSiteSubdirectory + "/",
		DataHost = Utils.AboutThisApp.AppLinksBaseDomain)]
    [IntentFilter(new []{ Intent.ActionView },
        Categories = new []
        {
            Intent.CategoryDefault,
            Intent.CategoryBrowsable
        },
        DataScheme = "https",
        DataPathPrefix = "/" + Utils.AboutThisApp.SessionsSiteSubdirectory + "/",
        DataHost = Utils.AboutThisApp.AppLinksBaseDomain)]
    public class MainActivity : FormsAppCompatActivity
    {
		private const string TAG = "MainActivity";
		public const string CHANNEL_ID = "Announcements";
		protected override void OnCreate (Bundle savedInstanceState)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(savedInstanceState);

			Forms.Init(this, savedInstanceState);
			FormsMaps.Init(this, savedInstanceState);
			AndroidAppLinks.Init(this);
			Toolkit.Init();

			PullToRefreshLayoutRenderer.Init();

			ImageCircle.Forms.Plugin.Droid.ImageCircleRenderer.Init();

			LoadApplication(new App());

			InitializePushNotifications();
			var gpsAvailable = IsPlayServicesAvailable();
			Settings.Current.PushNotificationsEnabled = gpsAvailable;

			OnNewIntent(Intent);


			if (!string.IsNullOrWhiteSpace(Intent?.Data?.LastPathSegment))
			{

				switch (Intent.Data.LastPathSegment)
				{
					case "sessions":
						MessagingService.Current.SendMessage<DeepLinkPage>("DeepLinkPage", new DeepLinkPage
						{
							Page = AppPage.Sessions
						});
						break;
					case "events":
						MessagingService.Current.SendMessage<DeepLinkPage>("DeepLinkPage", new DeepLinkPage
						{
							Page = AppPage.Events
						});
						break;
					case "minihacks":
						MessagingService.Current.SendMessage<DeepLinkPage>("DeepLinkPage", new DeepLinkPage
						{
							Page = AppPage.MiniHacks
						});
						break;
				}
			}

			DataRefreshService.ScheduleRefresh(this);

			if (!Settings.Current.PushNotificationsEnabled)
				return;


		}

		private void InitializePushNotifications()
		{
			if (Intent.Extras != null)
			{
				foreach (var key in Intent.Extras.KeySet())
				{
					if (key != null)
					{
						var value = Intent.Extras.GetString(key);
						Log.Debug(TAG, "Key: {0} Value: {1}", key, value);
					}
				}
			}

			IsPlayServicesAvailable();
			CreateNotificationChannel();
		}

	    public bool IsPlayServicesAvailable ()
        {
			
			int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
			if (resultCode != ConnectionResult.Success)
			{
				if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
					Log.Debug(TAG, GoogleApiAvailability.Instance.GetErrorString(resultCode));
				else
				{
					Log.Debug(TAG, "This device is not supported");
					Finish();
				}
				return false;
			}

			Log.Debug(TAG, "Google Play Services is available.");
			return true;
		}

		private void CreateNotificationChannel()
		{
			
			if (Build.VERSION.SdkInt < BuildVersionCodes.O)
			{
				// Notification channels are new in API 26 (and not a part of the
				// support library). There is no need to create a notification
				// channel on older versions of Android.
				return;
			}

			var channelName = CHANNEL_ID;
			var channelDescription = string.Empty;
			var channel = new NotificationChannel(CHANNEL_ID, channelName, NotificationImportance.Default)
			{
				Description = channelDescription
			};

			var notificationManager = (NotificationManager)GetSystemService(NotificationService);
			notificationManager.CreateNotificationChannel(channel);
		}

	}
}

