using FormsToolkit;
using MvvmHelpers;
using Xamarin.Forms;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
    public class AboutViewModel : SettingsViewModel
    {        
        public ObservableRangeCollection<Grouping<string, MenuItem>> MenuItems { get; }
        public ObservableRangeCollection<MenuItem> InfoItems { get; } = new ObservableRangeCollection<MenuItem>();
        public ObservableRangeCollection<MenuItem> AccountItems { get; } = new ObservableRangeCollection<MenuItem>();
		public ObservableRangeCollection<MenuItem> PrivacyItems { get; } = new ObservableRangeCollection<MenuItem>();

		MenuItem syncItem;
        MenuItem accountItem;
        MenuItem pushItem;
		IPushNotifications push;

        public AboutViewModel()
        {
            AboutItems.Clear();
            push = Locator.Get<IPushNotifications>();

            InfoItems.Add(new MenuItem { Name = $"Venue: {EventInfo.VenueName}", Icon = "icon_venue.png", Parameter = "venue" });

            if (!FeatureFlags.SponsorsOnTabPage)
            {
                InfoItems.Add(new MenuItem { Name = "Sponsors", Icon = "icon_venue.png", Parameter = "sponsors" });
            }
            if (FeatureFlags.EvalEnabled)
            {
                InfoItems.Add(new MenuItem { Name = "Evaluations", Icon = "icon_venue.png", Parameter = "evaluations" });
            }
            if (FeatureFlags.FloormapEnabled)
            {
                InfoItems.Add(new MenuItem { Name = "Conference Floor Maps", Icon = "icon_venue.png", Parameter = "floor-maps" });
            }

            PrivacyItems.Add(new MenuItem { Name = "Privacy policy", Icon = "icon_privacy.png", Parameter = "privacy" });

            if (FeatureFlags.CodeOfConductEnabled)
            {
                PrivacyItems.Add(new MenuItem { Name = AboutThisApp.CodeOfConductPageTitle, Icon = "icon_code_of_conduct.png", Parameter = "code-of-conduct" });
            }
            if (FeatureFlags.WifiEnabled)
            {
                InfoItems.Add(new MenuItem { Name = "Wi-Fi Information", Icon = "icon_wifi.png", Parameter = "wi-fi" });
            }
            accountItem = new MenuItem
            {
                Name = "Logged in as:"
            };

            syncItem = new MenuItem
            {
                Name = "Last Sync:"
            };

            pushItem = new MenuItem
            {
                Name = "Enable push notifications"
            };

            pushItem.Command = new Command(() =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (push.IsRegistered)
                        {
                            UpdateItems();
                            return;
                        }

                        if (Settings.AttemptedPush)
                        {
                            MessagingService.Current.SendMessage<MessagingServiceQuestion>(MessageKeys.Question, new MessagingServiceQuestion
                            {
                                Title = "Push Notification",
                                Question = "To enable push notifications, please go into Settings, Tap Notifications, and set Allow Notifications to on.",
                                Positive = "Settings",
                                Negative = "Maybe Later",
                                OnCompleted = (result) =>
                                    {
                                        if (result)
                                        {
                                            push.OpenSettings();
                                        }
                                    }
                            });
                            return;
                        }

                        push.RegisterForNotifications();
                    });
                });

            UpdateItems();

			if (FeatureFlags.LoginEnabled)
			{
				AccountItems.Add(accountItem);
			}
            AccountItems.Add(syncItem);
            AccountItems.Add(pushItem);

			if (!FeatureFlags.LoginEnabled && FeatureFlags.AppToWebLinkingEnabled)
			{
				AccountItems.Add(new MenuItem { Name = "Link app data to website", Icon = "icon_linkapptoweb.png", Parameter = "mobiletowebsync" });
				AccountItems.Add(new MenuItem { Name = "Link website data to app", Icon = "icon_linkapptoweb.png", Parameter = "webtomobilesync" });
			}

			AboutItems.Add(new MenuItem { Name = "About this app", Icon = "heart_filled.png" });

			//This will be triggered wen 
			Settings.PropertyChanged += (sender, e) => 
                {
                    if(e.PropertyName == "Email" || e.PropertyName == "LastSync" || e.PropertyName == "PushNotificationsEnabled")
                    {
                        UpdateItems();
                        OnPropertyChanged("AccountItems");
                    }
                };
        }

        public void UpdateItems()
        {
            syncItem.Subtitle = LastSyncDisplay;
			if (FeatureFlags.LoginEnabled)
			{
				accountItem.Subtitle = Settings.Current.IsLoggedIn ? Settings.Current.UserDisplayName : "Not signed in";
			}
			else
			{
				accountItem.Subtitle = "";
			}

            Device.BeginInvokeOnMainThread(() =>
            {
                pushItem.Name = push.IsRegistered ? "Push notifications enabled" : "Enable push notifications";
            });
        }

    }
}

