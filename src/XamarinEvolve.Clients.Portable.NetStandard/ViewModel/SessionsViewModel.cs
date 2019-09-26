using System;
using System.Linq;
using Xamarin.Forms;
using System.Windows.Input;
using System.Threading.Tasks;
using MvvmHelpers;
using FormsToolkit;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
    public class SessionsViewModel : ViewModelBase
    {
        public SessionsViewModel(INavigation navigation) : base(navigation)
        {
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
            favoriteCommand?.ChangeCanExecute();
            forceRefreshCommand?.ChangeCanExecute();
            loadSessionsCommand?.ChangeCanExecute();
            filterSessionsCommand?.ChangeCanExecute();
        }

		void UpdateFavoritedSession(IMessagingService service, Session updatedSession)
		{
			var sessionInList = SessionsGrouped.SelectMany(g => g).FirstOrDefault(s => s.Id == updatedSession.Id);
			if (sessionInList != null && sessionInList.IsFavorite != updatedSession.IsFavorite)
			{
				sessionInList.IsFavorite = updatedSession.IsFavorite;
			}
		}

		public ObservableRangeCollection<Session> Sessions { get; } = new ObservableRangeCollection<Session>();
        public ObservableRangeCollection<Session> SessionsFiltered { get; } = new ObservableRangeCollection<Session>();
        public ObservableRangeCollection<Grouping<string, Session>> SessionsGrouped { get; } = new ObservableRangeCollection<Grouping<string, Session>>();

        #region Properties
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

        string filter = string.Empty;
        public string Filter
        {
            get { return filter; }
            set 
            {
                if (SetProperty(ref filter, value))
                    ExecuteFilterSessionsAsync().IgnoreResult();                    
            }
        }
        #endregion

        #region Filtering and Sorting

        void SortSessions()
        {
            var grouped = SessionsFiltered.FilterAndGroupByDate();
            SessionsGrouped.ReplaceRange(grouped);
        }

        bool noSessionsFound;
        public bool NoSessionsFound
        {
            get { return noSessionsFound; }
            set { SetProperty(ref noSessionsFound, value); }
        }

        string noSessionsFoundMessage;
        public string NoSessionsFoundMessage
        {
            get { return noSessionsFoundMessage; }
            set { SetProperty(ref noSessionsFoundMessage, value); }
        }
 
        #endregion

        #region Commands

        Command forceRefreshCommand;
        public ICommand ForceRefreshCommand =>
            forceRefreshCommand ?? (forceRefreshCommand = new Command(() => ExecuteForceRefreshCommandAsync().IgnoreResult(ShowError), () => !IsBusy)); 

        async Task ExecuteForceRefreshCommandAsync()
        {
            await ExecuteLoadSessionsAsync(true);
        }

        Command filterSessionsCommand;
        public ICommand FilterSessionsCommand =>
            filterSessionsCommand ?? (filterSessionsCommand = new Command(() => ExecuteFilterSessionsAsync().IgnoreResult(ShowError), () => !IsBusy)); 

        async Task ExecuteFilterSessionsAsync()
        {
            NoSessionsFound = false;

            // Abort the current command if the user is typing fast
            if (!string.IsNullOrEmpty(Filter))
            {
				var query = Filter;
                await Task.Delay(AppBehavior.DelayBeforeSearchingInMilliseconds);
                if (query != Filter) 
                    return;
            }

            SessionsFiltered.ReplaceRange(Sessions.Search(Filter));
            SortSessions();

            if(SessionsGrouped.Count == 0)
            {
                if(Settings.Current.FavoritesOnly)
                {
                    if(!Settings.Current.ShowPastSessions && !string.IsNullOrWhiteSpace(Filter))
                        NoSessionsFoundMessage = "You haven't favorited\nany sessions yet.";
                    else
                        NoSessionsFoundMessage = "No Sessions Found";
                }
                else
                    NoSessionsFoundMessage = "No Sessions Found";

                NoSessionsFound = true;
            }
            else
            {
                NoSessionsFound = false;
            }
		}

        Command loadSessionsCommand;
        public ICommand LoadSessionsCommand =>
            loadSessionsCommand ?? (loadSessionsCommand = new Command<bool>((f) => ExecuteLoadSessionsAsync().IgnoreResult(ShowError), (o) => !IsBusy));

        async Task<bool> ExecuteLoadSessionsAsync(bool force = false)
        {
            if(IsBusy)
                return false;

            try 
            {
                IsBusy = true;

				NoSessionsFound = false;
                Filter = string.Empty;

                force = force || NextForceRefresh <= Clock.Now;
				if (force)
				{
					NextForceRefresh = Clock.Now.AddMinutes(AppBehavior.RefreshIntervalInMinutes);
				}

				Sessions.ReplaceRange(await StoreManager.SessionStore.GetItemsAsync(force));
                SessionsFiltered.ReplaceRange(Sessions);
				SortSessions();

				if(SessionsGrouped.Count == 0)
                {                    
                    if(Settings.Current.FavoritesOnly)
                    {
                        if(!Settings.Current.ShowPastSessions)
                            NoSessionsFoundMessage = "You haven't favorited\nany sessions yet.";
                        else
                            NoSessionsFoundMessage = "No Sessions Found";
                    }
                    else
                        NoSessionsFoundMessage = "No Sessions Found";

                    NoSessionsFound = true;
                }
                else
                {
                    NoSessionsFound = false;
                }
            } 
            catch (Exception ex) 
            {
                Logger.Report(ex, "Method", "ExecuteLoadSessionsAsync");
                MessagingService.Current.SendMessage(MessageKeys.Error, ex);
            }
            finally
            {
                IsBusy = false;
			}

            return true;
        }

        Command  favoriteCommand;
        public ICommand FavoriteCommand =>
            favoriteCommand ?? (favoriteCommand = new Command<Session>((s) => ExecuteFavoriteCommandAsync(s).IgnoreResult(ShowError), (arg) => !IsBusy)); 

        async Task ExecuteFavoriteCommandAsync(Session session)
        {
            var toggled = await FavoriteService.ToggleFavorite(session);
            if (toggled && Settings.Current.FavoritesOnly)
            {
                SortSessions();
            }
        }

        #endregion
    }
}

