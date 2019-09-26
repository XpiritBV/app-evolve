using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using XamarinEvolve.DataObjects;
using System.Windows.Input;
using Plugin.Share;
using FormsToolkit;
using MvvmHelpers;
using Plugin.Share.Abstractions;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
	public class SessionDetailsViewModel : ViewModelBase
	{
		Session session;
        IRoomStore _roomStore;
		IAppVersionProvider _appVersionProvider;

		public Session Session
		{
			get { return session; }
			set { SetProperty(ref session, value); }
		}

		public ObservableRangeCollection<MenuItem> SessionMaterialItems { get; } = new ObservableRangeCollection<MenuItem>();

		public SessionDetailsViewModel(INavigation navigation, Session session) : base(navigation)
		{
            _roomStore = Locator.Get<IRoomStore>();
			_appVersionProvider = Locator.Get<IAppVersionProvider>();
			Session = session;
			if (Session.StartTime.HasValue)
                ShowReminder = !Session.StartTime.Value.IsTBA() && !Session.IsInPast;
			else
				ShowReminder = false;

#if DEBUG
			if (string.IsNullOrWhiteSpace(session.PresentationUrl))
				session.PresentationUrl = "http://www.xamarin.com";

			if (string.IsNullOrWhiteSpace(session.VideoUrl))
				session.VideoUrl = "http://www.xamarin.com";

			if (string.IsNullOrWhiteSpace(session.AudioStreamWebUrl))
				session.AudioStreamWebUrl = "http://www.xamarin.com";
#endif

			if (!string.IsNullOrWhiteSpace(session.PresentationUrl))
			{
				SessionMaterialItems.Add(new MenuItem
				{
					Name = "Presentation Slides",
					Parameter = session.PresentationUrl,
					Icon = "icon_presentation.png"
				});
			}

			if (!string.IsNullOrWhiteSpace(session.VideoUrl))
			{
				SessionMaterialItems.Add(new MenuItem
				{
					Name = "Session Recording",
					Parameter = session.VideoUrl,
					Icon = "icon_video.png"
				});
			}

			MessagingService.Current.Subscribe<Session>(MessageKeys.SessionFavoriteToggled, UpdateFavoritedSession);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MessagingService.Current.Unsubscribe<Session>(MessageKeys.SessionFavoriteToggled);
			}
		}

		protected override void UpdateCommandCanExecute()
        {
            shareCommand?.ChangeCanExecute();
            favoriteCommand?.ChangeCanExecute();
            reminderCommand?.ChangeCanExecute();
            goToRoomCommand?.ChangeCanExecute();
            loadSessionCommand?.ChangeCanExecute();
			openAudioStreamCommand?.ChangeCanExecute();
        }

#if DEBUG
		public bool ShowSessionMaterials => false;
		public bool ShowAudioStream => false;
#else
		public bool ShowSessionMaterials => false;
		public bool ShowAudioStream => false;
#endif

		MenuItem selectedSessionMaterialItem;
		public MenuItem SelectedSessionMaterialItem
		{
			get { return selectedSessionMaterialItem; }
			set
			{
				selectedSessionMaterialItem = value;
				OnPropertyChanged();
				if (selectedSessionMaterialItem == null)
					return;

				LaunchBrowserCommand.Execute(selectedSessionMaterialItem.Parameter);

				SelectedSessionMaterialItem = null;
			}
		}

		void UpdateFavoritedSession(IMessagingService service, Session updatedSession)
		{
			if (Session.Id == updatedSession.Id && Session.IsFavorite != updatedSession.IsFavorite)
			{
				Session.IsFavorite = updatedSession.IsFavorite;
			}
		}

		public bool ShowReminder { get; set; }

		bool isReminderSet;
		public bool IsReminderSet
		{
			get { return isReminderSet; }
			set { SetProperty(ref isReminderSet, value); }
		}

		Command favoriteCommand;
		public ICommand FavoriteCommand =>
            favoriteCommand ?? (favoriteCommand = new Command(() => ExecuteFavoriteCommandAsync().IgnoreResult()));

		async Task ExecuteFavoriteCommandAsync()
		{
			await FavoriteService.ToggleFavorite(Session);
		}

		Command openAudioStreamCommand;
		public ICommand OpenAudioStreamCommand =>
			openAudioStreamCommand ?? (openAudioStreamCommand = new Command(() => LaunchBrowserCommand.Execute(session.AudioStreamWebUrl), () => !IsBusy));

		Command reminderCommand;
		public ICommand ReminderCommand =>
			reminderCommand ?? (reminderCommand = new Command(() => ExecuteReminderCommandAsync().IgnoreResult()));

		async Task ExecuteReminderCommandAsync()
		{
			if (!IsReminderSet)
			{
                var result = await ReminderService.AddReminderAsync(Session.Id,
					new Plugin.Calendars.Abstractions.CalendarEvent
					{
						AllDay = false,
						Description = Session.Abstract,
						Location = Session.Room?.Name ?? string.Empty,
						Name = Session.Title,
						Start = Session.StartTime.Value,
						End = Session.EndTime.Value
					});

				if (!result)
					return;

				Logger.Track(EvolveLoggerKeys.ReminderAdded, "Title", Session.Title);
				IsReminderSet = true;
			}
			else
			{
                var result = await ReminderService.RemoveReminderAsync(Session.Id);
                if (!result)
                {
                    return;
                }
				Logger.Track(EvolveLoggerKeys.ReminderRemoved, "Title", Session.Title);
				IsReminderSet = false;
			}

		}

		Command goToRoomCommand;
		public ICommand GoToRoomCommand =>
            goToRoomCommand ?? (goToRoomCommand = new Command(ExecuteGoToRoom));

		void ExecuteGoToRoom()
        {
			if (FeatureFlags.FloormapEnabled)
			{
				MessagingService.Current.SendMessage(MessageKeys.NavigateToRoom, Session.Room);
			}
		}

		Command shareCommand;
		public ICommand ShareCommand =>
			shareCommand ?? (shareCommand = new Command(() => ExecuteShareCommandAsync().IgnoreResult()));

		async Task ExecuteShareCommandAsync()
		{
			Logger.Track(EvolveLoggerKeys.Share, "Title", Session.Title);
			var speakerHandles = Session.SpeakerHandles;
            string url = null;

            if (!string.IsNullOrEmpty(speakerHandles))
			{
				speakerHandles = " by " + speakerHandles;
			}
			var message = $"Can't wait for {Session.Title}{speakerHandles} at {EventInfo.HashTag}!";

            if (FeatureFlags.AppLinksEnabled && Device.RuntimePlatform != Device.Android)
			{
                url = Session.GetWebUrl();
				message += " " + url;
			}
            var shareMessage = new ShareMessage { Text = message, Title = "Share", Url = url };

            await CrossShare.Current.Share(shareMessage);
		}

		Command loadSessionCommand;
		public ICommand LoadSessionCommand =>
            loadSessionCommand ?? (loadSessionCommand = new Command(() => ExecuteLoadSessionCommandAsync().IgnoreResult(ShowError), () => !IsBusy));

		public async Task ExecuteLoadSessionCommandAsync()
		{
			if (IsBusy)
				return;

			try
			{
				IsBusy = true;

                IsReminderSet = await ReminderService.HasReminderAsync(Session.Id);
				Session.FeedbackLeft = await StoreManager.FeedbackStore.LeftFeedback(Session);
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadSessionCommandAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}

