using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FormsToolkit;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using XamarinEvolve.DataStore.Azure;
//using Microsoft.AppCenter.Crashes;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace XamarinEvolve.Clients.UI
{
	public partial class App : Application
	{
		public static App CurrentApplication;

		public App()
		{
			//simulate we are on the event date
			Clock.Now = new DateTime(2019, 10, 1, 15, 0, 0, DateTimeKind.Utc);

			CurrentApplication = this;
			InitializeComponent();
			Init();


			// The root page of your application
			switch (Xamarin.Forms.Device.RuntimePlatform)
			{
				case Xamarin.Forms.Device.Android:
					MainPage = new RootPageAndroid();
					break;
				case Xamarin.Forms.Device.iOS:
					MainPage = new EvolveNavigationPage(new RootPageiOS());
					break;
				default:
					throw new NotImplementedException();
			}


		}

		static ILogger logger;
		public static ILogger Logger => logger ?? (logger = DependencyService.Get<ILogger>());

		public static void Init()
		{

			DependencyService.Register<ISessionStore, XamarinEvolve.DataStore.Azure.SessionStore>();
			DependencyService.Register<IRoomStore, XamarinEvolve.DataStore.Azure.RoomStore>();
			DependencyService.Register<IFavoriteStore, XamarinEvolve.DataStore.Azure.FavoriteStore>();
			DependencyService.Register<IFeedbackStore, XamarinEvolve.DataStore.Azure.FeedbackStore>();
			DependencyService.Register<IConferenceFeedbackStore, XamarinEvolve.DataStore.Azure.ConferenceFeedbackStore>();
			DependencyService.Register<ISpeakerStore, XamarinEvolve.DataStore.Azure.SpeakerStore>();
			DependencyService.Register<ISponsorStore, XamarinEvolve.DataStore.Azure.SponsorStore>();
			DependencyService.Register<ICategoryStore, XamarinEvolve.DataStore.Azure.CategoryStore>();
			DependencyService.Register<IEventStore, XamarinEvolve.DataStore.Azure.EventStore>();
			DependencyService.Register<INotificationStore, XamarinEvolve.DataStore.Azure.NotificationStore>();
			DependencyService.Register<IMiniHacksStore, XamarinEvolve.DataStore.Azure.MiniHacksStore>();
			DependencyService.Register<IScavengerHuntStore, XamarinEvolve.DataStore.Azure.ScavengerHuntStore>();
			DependencyService.Register<IAttendeeStore, XamarinEvolve.DataStore.Azure.AttendeeStore>();
			DependencyService.Register<ISSOClient, XamarinEvolve.Clients.Portable.Auth.Azure.XamarinSSOClient>();
			DependencyService.Register<IStoreManager, XamarinEvolve.DataStore.Azure.StoreManager>();
			DependencyService.Register<FavoriteService>();
		}

		void TaskErrorHandler(Exception e)
		{
			Logger.Report(e, Severity.Warning);
		}

		public void SecondOnResume()
		{
			OnResume();
		}

		bool registered;
		bool firstRun = true;

		protected override void OnStart()
		{
			OnResume();
		}

		protected override void OnResume()
		{
			Debug.WriteLine($"Current user: {Settings.Current.UserIdentifier}");

			if (registered)
			{
				return;
			}

			registered = true;

			if (Settings.Current.FirstRun)
			{
				if (EventInfo.EndOfConference <= Clock.Now)
				{
					Settings.Current.ShowPastSessions = true;
				}
			}

			// Handle when your app resumes
			Settings.Current.IsConnected = CrossConnectivity.Current.IsConnected;
			CrossConnectivity.Current.ConnectivityChanged += ConnectivityChanged;

			// Handle when your app starts
			MessagingService.Current.Subscribe<MessagingServiceAlert>(MessageKeys.Message, async (m, info) =>
				{
					var task = Application.Current?.MainPage?.DisplayAlert(info.Title, info.Message, info.Cancel);

					if (task == null)
						return;

					await task;
					info?.OnCompleted?.Invoke();
				});

			MessagingService.Current.Subscribe<MessagingServiceQuestion>(MessageKeys.Question, async (m, q) =>
				{
					var task = Application.Current?.MainPage?.DisplayAlert(q.Title, q.Question, q.Positive, q.Negative);
					if (task == null)
						return;
					var result = await task;
					q?.OnCompleted?.Invoke(result);
				});

			MessagingService.Current.Subscribe<MessagingServiceChoice>(MessageKeys.Choice, async (m, q) =>
				{
					var task = Application.Current?.MainPage?.DisplayActionSheet(q.Title, q.Cancel, q.Destruction, q.Items);
					if (task == null)
						return;
					var result = await task;
					q?.OnCompleted?.Invoke(result);
				});

			MessagingService.Current.Subscribe(MessageKeys.NavigateLogin, async m =>
				{
					if (FeatureFlags.LoginEnabled)
					{
						if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
						{
							((RootPageAndroid)MainPage).IsPresented = false;
						}

						Page page = null;
						if (Settings.Current.FirstRun && Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
							page = new LoginPage();
						else
							page = new EvolveNavigationPage(new LoginPage());

						var nav = Application.Current?.MainPage?.Navigation;
						if (nav == null)
							return;

						await NavigationService.PushModalAsync(nav, page);
					}
					else
					{
						await PerformAnonymousLogin();
					}
				});

			try
			{
				if (firstRun && Settings.Current.NeedsSync)
				{
					PerformSyncInBackground(true);
				}

				if (firstRun || Xamarin.Forms.Device.RuntimePlatform != Xamarin.Forms.Device.iOS)
					return;

				var mainNav = MainPage as NavigationPage;
				if (mainNav == null)
					return;

				var rootPage = mainNav.CurrentPage as RootPageiOS;
				if (rootPage == null)
					return;

				var rootNav = rootPage.CurrentPage as NavigationPage;
				if (rootNav == null)
					return;

				var about = rootNav.CurrentPage as AboutPage;
				if (about != null)
				{
					about.OnResume();
					return;
				}
				var sessions = rootNav.CurrentPage as SessionsPage;
				if (sessions != null)
				{
					sessions.OnResume();
					return;
				}
				var feed = rootNav.CurrentPage as FeedPage;
				if (feed != null)
				{
					feed.OnResume();
					return;
				}
			}
			catch (Exception ex)
			{
				Logger.Report(ex, Severity.Warning);
			}
			finally
			{
				firstRun = false;
			}
		}

		public async Task PerformAnonymousLogin()
		{
			var ssoClient = DependencyService.Get<ISSOClient>();

			if (ssoClient != null)
			{
				var account = await ssoClient.LoginAnonymouslyAsync(null);
				if (account != null)
				{
					Settings.Current.UserIdentifier = account.User.Email;
					MessagingService.Current.SendMessage(MessageKeys.LoggedIn);
					Logger.Track(EvolveLoggerKeys.LoginSuccess);
				}

				PerformSyncInBackground();
			}
			await Finish();
			Settings.Current.FirstRun = false;
		}

		private bool _syncing;
		void PerformSyncInBackground(bool showToast = false)
		{
			if (_syncing)
				return;

			if (CrossConnectivity.Current.IsConnected && showToast)
			{
				var toaster = DependencyService.Get<IToast>();
				toaster.SendToast("Syncing data in the background to make sure you're up to date. It may take a while to load all sessions and speakers.");
			}

			Task.Run(async () =>
			{
				_syncing = true;
				var storeManager = DependencyService.Get<IStoreManager>();

				if (await storeManager.SyncAllAsync(Settings.Current.IsLoggedIn))
				{
					Settings.Current.LastSync = Clock.Now;
					Settings.Current.HasSyncedData = true;
				}
				_syncing = false;
			}).IgnoreResult(ReportError);
		}

		void ReportSyncError(Exception ex)
		{
			Logger.Report(ex);
			_syncing = false;
		}

		static void ReportError(Exception ex)
		{
			Logger.Report(ex);
		}

		async Task Finish()
		{
			if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS && Settings.Current.FirstRun)
			{
				var push = DependencyService.Get<IPushNotifications>();
				if (push != null)
					await push.RegisterForNotifications();
			}
		}


		protected override void OnAppLinkRequestReceived(Uri uri)
		{
			var data = uri.ToString().ToLowerInvariant();

			//only if deep linking
			if (!data.Contains($"/{AboutThisApp.SessionsSiteSubdirectory.ToLowerInvariant()}/")
				&& !data.Contains($"/{AboutThisApp.SpeakersSiteSubdirectory.ToLowerInvariant()}/")
				&& !data.Contains($"/{AboutThisApp.MiniHacksSiteSubdirectory.ToLowerInvariant()}/"))
				return;

			var id = data.Substring(data.LastIndexOf("/", StringComparison.Ordinal) + 1);

			if (!string.IsNullOrWhiteSpace(id))
			{
				AppPage destination = AppPage.Session;
				if (data.Contains($"/{AboutThisApp.SpeakersSiteSubdirectory.ToLowerInvariant()}/"))
				{
					destination = AppPage.Speaker;
				}
				if (data.Contains($"/{AboutThisApp.MiniHacksSiteSubdirectory.ToLowerInvariant()}/"))
				{
					destination = AppPage.MiniHack;
				}
				MessagingService.Current.SendMessage("DeepLinkPage", new DeepLinkPage
				{
					Page = destination,
					Id = id.Replace("#", "")
				});
			}

			base.OnAppLinkRequestReceived(uri);
		}

		protected override void OnSleep()
		{
			if (!registered)
				return;

			registered = false;
			MessagingService.Current.Unsubscribe(MessageKeys.NavigateLogin);
			MessagingService.Current.Unsubscribe<MessagingServiceQuestion>(MessageKeys.Question);
			MessagingService.Current.Unsubscribe<MessagingServiceAlert>(MessageKeys.Message);
			MessagingService.Current.Unsubscribe<MessagingServiceChoice>(MessageKeys.Choice);

			// Handle when your app sleeps
			CrossConnectivity.Current.ConnectivityChanged -= ConnectivityChanged;
		}

		protected void ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
		{
			//save current state and then set it
			var connected = Settings.Current.IsConnected;
			Settings.Current.IsConnected = e.IsConnected;
			if (connected && !e.IsConnected)
			{
				var toaster = DependencyService.Get<IToast>();
				toaster.SendToast("Uh Oh, It looks like you have gone offline. Check your internet connection to get the latest data.");
			}
		}

	}
}

