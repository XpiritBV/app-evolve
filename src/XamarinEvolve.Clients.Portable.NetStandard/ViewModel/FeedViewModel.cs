using System;
using System.Windows.Input;
using System.Threading.Tasks;
using MvvmHelpers;
using Xamarin.Forms;
using FormsToolkit;
using System.Reflection;
using XamarinEvolve.Utils.Helpers;
using Newtonsoft.Json;
using XamarinEvolve.DataObjects;
using System.Collections.Generic;
using XamarinEvolve.Utils;
using XamarinEvolve.DataStore.Abstractions;

namespace XamarinEvolve.Clients.Portable
{
	public class FeedViewModel : ViewModelBase
	{
		private IAppVersionProvider _appVersionProvider;
		public ObservableRangeCollection<Tweet> Tweets { get; } = new ObservableRangeCollection<Tweet>();
		public ObservableRangeCollection<Session> Sessions { get; } = new ObservableRangeCollection<Session>();

		public FeedViewModel()
		{
			_appVersionProvider = Locator.Get<IAppVersionProvider>();
#if DEBUG
			Title = $"{Utils.EventInfo.EventName} @ {Clock.Now.ToEventTimeZone().ToString("t")}";
#else
			Title = $"{Utils.EventInfo.EventName}";
#endif
			MessagingService.Current.Subscribe("conferencefeedback_finished", (m) => { Device.BeginInvokeOnMainThread(EvaluateVisualState); });
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MessagingService.Current.Unsubscribe("conferencefeedback_finished");
			}
		}

		protected override void UpdateCommandCanExecute()
        {
            shareCommand?.ChangeCanExecute();
            refreshCommand?.ChangeCanExecute();
            favoriteCommand?.ChangeCanExecute();
            loadSocialCommand?.ChangeCanExecute();
            buyTicketNowCommand?.ChangeCanExecute();
			openAudioStreamCommand?.ChangeCanExecute();
            loadSessionsCommand?.ChangeCanExecute();
            loadNotificationsCommand?.ChangeCanExecute();
            showConferenceFeedbackCommand?.ChangeCanExecute();
        }

		// only start showing upcoming favorites 1 day before the conference
		public bool ShowUpcomingFavorites = true;//=> Utils.EventInfo.StartOfConference.AddDays(-1) < Clock.Now;

        Command refreshCommand;
        public ICommand RefreshCommand =>
            refreshCommand ?? (refreshCommand = new Command(() => ExecuteRefreshCommandAsync().IgnoreResult(ShowError), () => !IsBusy)); 

        async Task ExecuteRefreshCommandAsync()
        {
			if (IsBusy)
				return;

            try
            {
				IsBusy = true;

				NextForceRefresh = Clock.Now.AddMinutes(AppBehavior.RefreshIntervalInMinutes);
                var tasks = new Task[]
                    {
                        ExecuteLoadNotificationsCommandAsync(),
                        ExecuteLoadSocialCommandAsync(),
                        ExecuteLoadSessionsCommandAsync()
                    };

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                ex.Data["method"] = "ExecuteRefreshCommandAsync";
                Logger.Report(ex);
            }
            finally
            {
				LoadingSocial = false;
				LoadingNotifications = false;
				LoadingSessions = false;
                IsBusy = false;
            }
        }

        Notification notification;
        public Notification Notification
        {
            get { return notification; }
            set { SetProperty(ref notification, value); }
        }

        bool loadingNotifications;
        public bool LoadingNotifications
        {
            get { return loadingNotifications; }
            set { SetProperty(ref loadingNotifications, value); }
        }

        Command  loadNotificationsCommand;
        public ICommand LoadNotificationsCommand =>
            loadNotificationsCommand ?? (loadNotificationsCommand = new Command(() => ExecuteLoadNotificationsCommandAsync().IgnoreResult(ShowError))); 

        async Task ExecuteLoadNotificationsCommandAsync()
        {
            if (LoadingNotifications)
                return;

            LoadingNotifications = true;

            try
            {
                Notification = await StoreManager.NotificationStore.GetLatestNotification().ConfigureAwait(false);
				if (notification == null)
				{
					Notification = new Notification
					{
						Date = Clock.Now,
						Text = $"Welcome to {Utils.EventInfo.EventName}!"
					};
				}
			}
            catch(Exception ex)
            {
                ex.Data["method"] = "ExecuteLoadNotificationsCommandAsync";
                Logger.Report(ex);
                 
            }
            finally
            {
                LoadingNotifications = false;
            }
        }

        bool loadingSessions;
        public bool LoadingSessions
        {
            get { return loadingSessions; }
            set { SetProperty(ref loadingSessions, value); }
        }

        Command  loadSessionsCommand;
        public ICommand LoadSessionsCommand =>
            loadSessionsCommand ?? (loadSessionsCommand = new Command(() => ExecuteLoadSessionsCommandAsync().IgnoreResult())); 

        async Task ExecuteLoadSessionsCommandAsync()
        {
			if (!ShowUpcomingFavorites)
				return;
			
            if (LoadingSessions)
                return;
            
            LoadingSessions = true;

			try
			{
				NoSessions = false;
				Sessions.Clear();
				OnPropertyChanged("Sessions");

                var sessions = await StoreManager.SessionStore.GetNextSessions(2).ConfigureAwait(false);
				if (sessions != null)
					Sessions.AddRange(sessions);

				NoSessions = Sessions.Count == 0;
			}
			catch (Exception ex)
			{
				ex.Data["method"] = "ExecuteLoadSessionsCommandAsync";
				Logger.Report(ex);
				NoSessions = true;
			}
			finally
			{
				LoadingSessions = false;
			}
        }

		public void EvaluateVisualState()
		{
			OnPropertyChanged(nameof(ShowBuyTicketButton));
			OnPropertyChanged(nameof(ShowUpcomingFavorites));
			OnPropertyChanged(nameof(ShowConferenceFeedbackButton));
		}

        bool noSessions;
        public bool NoSessions
        {
            get { return noSessions; }
            set { SetProperty(ref noSessions, value); }
        }

        Session selectedSession;
        public Session SelectedSession
        {
            get { return selectedSession; }
            set
            {
                selectedSession = value;
                OnPropertyChanged();
                if (selectedSession == null)
                    return;

                MessagingService.Current.SendMessage(MessageKeys.NavigateToSession, selectedSession);

                SelectedSession = null;
            }
        }

        bool loadingSocial;
        public bool LoadingSocial
        {
            get { return loadingSocial; }
            set { SetProperty(ref loadingSocial, value); }
        }

		public bool ShowBuyTicketButton => FeatureFlags.ShowBuyTicketButton && Utils.EventInfo.StartOfConference.AddDays(-1) >= Clock.Now;
		public string EventCodeText => $"Use event code {Utils.EventInfo.AudioStreamEventCode}";

#if DEBUG
		public bool ShowAudioStreamButton => FeatureFlags.AudioStreamEnabled;
		public bool ShowConferenceFeedbackButton => Utils.EventInfo.StartOfConference.AddHours(4) <= Clock.Now;// start as of afternoon of the last day with feedback collection
#else
		public bool ShowAudioStreamButton => FeatureFlags.AudioStreamEnabled && _appVersionProvider.SupportsWebRtc && Utils.EventInfo.StartOfConference <= Clock.Now;
		public bool ShowConferenceFeedbackButton => FeatureFlags.ShowConferenceFeedbackButton && Utils.EventInfo.EndOfConference.AddHours(-4) <= Clock.Now && !Settings.Current.IsConferenceFeedbackFinished();
#endif

		public string SocialHeader
		{
			get { return $"Social - {Utils.EventInfo.HashTag}"; }
		}

		Command shareCommand;
		public ICommand ShareCommand =>
            shareCommand ?? (shareCommand = new Command(() => ExecuteShareCommand().IgnoreResult(ShowError)));

		async Task ExecuteShareCommand()
		{
			var tweet = Locator.Get<ITweetService>();
			await tweet.InitiateConferenceTweet();
		}

		Command openAudioStreamCommand;
		public ICommand OpenAudioStreamCommand =>
		openAudioStreamCommand ?? (openAudioStreamCommand = new Command(ExecuteOpenAudioStreamCommand, () => !IsBusy));

		Command  buyTicketNowCommand;
        public ICommand BuyTicketNowCommand =>
			buyTicketNowCommand ?? (buyTicketNowCommand = new Command(ExecuteBuyTicketNowCommand, () => !IsBusy));

		void ExecuteOpenAudioStreamCommand()
		{
			LaunchBrowserCommand.Execute(Utils.EventInfo.AudioStreamUrl);
		}

		void ExecuteBuyTicketNowCommand()
		{
			LaunchBrowserCommand.Execute(Utils.EventInfo.TicketUrl);
		}

		Command showConferenceFeedbackCommand;
		public ICommand ShowConferenceFeedbackCommand =>
		showConferenceFeedbackCommand ?? (showConferenceFeedbackCommand = new Command(ExecuteShowConferenceFeedbackCommand, () => !IsBusy));

		void ExecuteShowConferenceFeedbackCommand()
		{
			MessagingService.Current.SendMessage(MessageKeys.NavigateToConferenceFeedback);
		}

		Command  loadSocialCommand;
        public ICommand LoadSocialCommand =>
            loadSocialCommand ?? (loadSocialCommand = new Command(() => ExecuteLoadSocialCommandAsync().IgnoreResult()));

        async Task ExecuteLoadSocialCommandAsync()
        {
            if (LoadingSocial)
                return;

            LoadingSocial = true;
            try
            {
                SocialError = false;
                Tweets.Clear();

#if ENABLE_TEST_CLOUD
                var json = ResourceLoader.GetEmbeddedResourceString(Assembly.Load(new AssemblyName("XamarinEvolve.Clients.Portable.NetStandard")), "sampletweets.txt");
                Tweets.ReplaceRange(JsonConvert.DeserializeObject<List<Tweet>>(json));
                await Task.Delay(10);
#else
                var manager = Locator.Get<IStoreManager>() as DataStore.Azure.StoreManager;
                if (manager == null)
                    return;

                await manager.InitializeAsync ().ConfigureAwait(false);

                var mobileClient = DataStore.Azure.StoreManager.MobileService;
                if (mobileClient == null)
                    return;
                
                var json = await mobileClient.InvokeApiAsync<string> ("Tweet", System.Net.Http.HttpMethod.Get, null).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(json)) 
                {
                    SocialError = true;
                    return;
                }

				var tweets = JsonConvert.DeserializeObject<List<Tweet>>(json);
				Device.BeginInvokeOnMainThread(() =>
				{
					Tweets.ReplaceRange(tweets);
				});
#endif
            }
            catch (Exception ex)
            {
                SocialError = true;
                ex.Data["method"] = "ExecuteLoadSocialCommandAsync";
                Logger.Report(ex);
            }
            finally
            {
                LoadingSocial = false;
            }
        }

        bool socialError;
        public bool SocialError
        {
            get { return socialError; }
            set { SetProperty(ref socialError, value); }
        }

		public string SocialErrorMessage => "Unable to load social feed";

		Tweet selectedTweet;
        public Tweet SelectedTweet
        {
            get { return selectedTweet; }
            set
            {
                selectedTweet = value;
                OnPropertyChanged();
                if (selectedTweet == null)
                    return;

                LaunchBrowserCommand.Execute(selectedTweet.Url);

                SelectedTweet = null;
            }
        }

        Command  favoriteCommand;
        public ICommand FavoriteCommand =>
        favoriteCommand ?? (favoriteCommand = new Command<Session>((s) => ExecuteFavoriteCommand(s))); 

		void ExecuteFavoriteCommand(Session session)
        {
			if (session.IsFavorite)
			{
				MessagingService.Current.SendMessage(MessageKeys.Question, new MessagingServiceQuestion
				{
					Negative = "Cancel",
					Positive = "Unfavorite",
					Question = "Are you sure you want to remove this session from your favorites?",
					Title = "Unfavorite Session",
					OnCompleted = ((result) =>
						{
							if (!result)
								return;

                            ToggleFavorite(session).IgnoreResult();
						})
				});
			}
        }

		async Task ToggleFavorite(Session session)
		{
			var toggled = await FavoriteService.ToggleFavorite(session);
			if (toggled)
			{
				await ExecuteLoadSessionsCommandAsync();
			}
		}
	}
}

