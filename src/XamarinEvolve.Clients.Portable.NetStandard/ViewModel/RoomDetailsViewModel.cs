using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FormsToolkit;
using MvvmHelpers;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
    public class RoomDetailsViewModel : ViewModelBase
    {
		public Room Room { get; set; }
		public ObservableRangeCollection<Session> Sessions { get; } = new ObservableRangeCollection<Session>();

        public RoomDetailsViewModel(Room room) : base()
        {
			Room = room;
			MessagingService.Current.Subscribe<Session>(MessageKeys.SessionFavoriteToggled, UpdateFavoritedSession);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MessagingService.Current.Unsubscribe<Session>(MessageKeys.SessionFavoriteToggled);
			}
		}

		public string SessionsInThisRoomText => Settings.Current.ShowPastSessions ? "Sessions in this room" : "Upcoming sessions in this room";
		public string FloorLevelText => $"Floor level: {Room?.FloorLevel + AppBehavior.FloorLevelCorrection}";

		protected override void UpdateCommandCanExecute()
        {
            loadSessionsCommand?.ChangeCanExecute();
			favoriteCommand?.ChangeCanExecute();
        }

		void UpdateFavoritedSession(IMessagingService service, Session updatedSession)
		{
			var sessionInList = Sessions.FirstOrDefault(s => s.Id == updatedSession.Id);
			if (sessionInList != null && sessionInList.IsFavorite != updatedSession.IsFavorite)
			{
				sessionInList.IsFavorite = updatedSession.IsFavorite;
			}
		}

		Command favoriteCommand;
		public ICommand FavoriteCommand =>
			favoriteCommand ?? (favoriteCommand = new Command<Session>((s) => ExecuteFavoriteCommandAsync(s).IgnoreResult(ShowError), (arg) => !IsBusy));

		async Task ExecuteFavoriteCommandAsync(Session session)
		{
			var toggled = await FavoriteService.ToggleFavorite(session);
			if (toggled && Settings.Current.FavoritesOnly)
			{
				loadSessionsCommand.Execute(null);
			}
		}

		Command loadSessionsCommand;
		public ICommand LoadSessionsCommand =>
            loadSessionsCommand ?? (loadSessionsCommand = new Command(() => ExecuteLoadSessionsCommandAsync().IgnoreResult(ShowError), () => !IsBusy));

		private async Task ExecuteLoadSessionsCommandAsync()
		{
            if (IsBusy)
            {
                return;
            }

			try
			{
				IsBusy = true;

				var items = (await StoreManager.SessionStore.GetRoomSessions(Room.Id));

				Sessions.ReplaceRange(items);
			}
			catch (Exception ex)
			{
				Logger.Report(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}
