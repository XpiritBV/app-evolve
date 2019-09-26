using System;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using System.Windows.Input;
using System.Threading.Tasks;
using MvvmHelpers;
using FormsToolkit;
using XamarinEvolve.Utils;
using System.Linq;

namespace XamarinEvolve.Clients.Portable
{
	public class SpeakerDetailsViewModel : ViewModelBase
    {
        
        public Speaker Speaker { get; set;}
        public ObservableRangeCollection<Session> Sessions { get; } = new ObservableRangeCollection<Session>();
        public ObservableRangeCollection<MenuItem> FollowItems { get; } = new ObservableRangeCollection<MenuItem>();

        bool hasAdditionalSessions;
        public bool HasAdditionalSessions
        {
            get { return hasAdditionalSessions; }
            set { SetProperty(ref hasAdditionalSessions, value); }
        }

        private string sessionId;
        public SpeakerDetailsViewModel(Speaker speaker, string sessionId) : base()
        {
            Speaker = speaker;
            this.sessionId = sessionId;

			MessagingService.Current.Subscribe<Session>(MessageKeys.SessionFavoriteToggled, UpdateFavoritedSession);

            if (!string.IsNullOrWhiteSpace(speaker.CompanyWebsiteUrl))
            {
                FollowItems.Add(new MenuItem
                    {
                        Name = speaker.CompanyWebsiteUrl.StripUrlForDisplay(),
                        Parameter = speaker.CompanyWebsiteUrl,
                        Icon = "icon_website.png"
                    });
            }

            if (!string.IsNullOrWhiteSpace(speaker.BlogUrl))
            {
                FollowItems.Add(new MenuItem
                    {
						Name = speaker.BlogUrl.StripUrlForDisplay(),
                        Parameter = speaker.BlogUrl,
                        Icon = "icon_blog.png"
                    });
            }

			if (!string.IsNullOrWhiteSpace(speaker.TwitterUrl))
			{
                var twitterValue = speaker.TwitterUrl.CleanUpTwitter(includeAtSign: false);

				FollowItems.Add(new MenuItem
				{
                    Name = speaker.TwitterHandle,
					Parameter = "https://twitter.com/" + twitterValue,
					Icon = "icon_twitter.png"
				});
			}
			if (!string.IsNullOrWhiteSpace(speaker.FacebookProfileName))
			{
				var profileName = speaker.FacebookProfileName.GetLastPartOfUrl();
				var profileDisplayName = profileName;
				Int64 testProfileId;
				if (Int64.TryParse(profileName, out testProfileId))
				{
					profileDisplayName = "Facebook";
				}
				FollowItems.Add(new MenuItem
				{
					Name = profileDisplayName,
					Parameter = "https://facebook.com/" + profileName,
					Icon = "icon_facebook.png"
				});
			}
			if (!string.IsNullOrWhiteSpace(speaker.LinkedInUrl))
			{
				FollowItems.Add(new MenuItem
				{
					Name = "LinkedIn",
					Parameter = "https://www.linkedin.com/in/" + speaker.LinkedInUrl.StripUrlForDisplay(),
					Icon = "icon_linkedin.png"
				});
			}
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
            favoriteCommand?.ChangeCanExecute();
            loadSessionsCommand?.ChangeCanExecute();
        }

		void UpdateFavoritedSession(IMessagingService service, Session updatedSession)
		{
			var sessionInList = Sessions.FirstOrDefault(s => s.Id == updatedSession.Id);
			if (sessionInList != null && sessionInList.IsFavorite != updatedSession.IsFavorite)
			{
				sessionInList.IsFavorite = updatedSession.IsFavorite;
			}
		}

		Command  loadSessionsCommand;
        public ICommand LoadSessionsCommand =>
            loadSessionsCommand ?? (loadSessionsCommand = new Command(() => ExecuteLoadSessionsCommandAsync().IgnoreResult(ShowError), () => !IsBusy)); 

        public async Task ExecuteLoadSessionsCommandAsync()
        {
            if(IsBusy)
                return;
            
            try
            {
                IsBusy = true;
			
				//var speaker = await StoreManager.SpeakerStore.GetItemAsync(Speaker.Id);
				//var items = speaker.Sessions;

				var items = (await StoreManager.SessionStore.GetSpeakerSessionsAsync(Speaker.Id));
				Sessions.ReplaceRange(items);
                HasAdditionalSessions = Sessions.Count > 0;
            }
            catch(Exception ex)
            {
                HasAdditionalSessions = false;  
                Logger.Report(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        MenuItem selectedFollowItem;
        public MenuItem SelectedFollowItem
        {
            get { return selectedFollowItem; }
            set
            {
                selectedFollowItem = value;
                OnPropertyChanged();
                if (selectedFollowItem == null)
                    return;

                LaunchBrowserCommand.Execute(selectedFollowItem.Parameter);

                SelectedFollowItem = null;
            }
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

        Command  favoriteCommand;
        public ICommand FavoriteCommand =>
            favoriteCommand ?? (favoriteCommand = new Command<Session>((s) => ExecuteFavoriteCommandAsync(s).IgnoreResult(ShowError), (arg) => !IsBusy)); 

		async Task ExecuteFavoriteCommandAsync(Session session)
        {
			if (session.IsFavorite)
			{
				MessagingService.Current.SendMessage<MessagingServiceQuestion>(MessageKeys.Question, new MessagingServiceQuestion
				{
					Negative = "Cancel",
					Positive = "Unfavorite",
					Question = "Are you sure you want to remove this session from your favorites?",
					Title = "Unfavorite Session",
					OnCompleted = ((result) =>
						{
							if (!result)
								return;
                            ToggleFavorite(session).IgnoreResult(ShowError);
					})
				});
			}
			else
			{
				await ToggleFavorite(session);
			}
        }

		async Task ToggleFavorite(Session session)
		{
			var toggled = await FavoriteService.ToggleFavorite(session);
		}
	}
}

