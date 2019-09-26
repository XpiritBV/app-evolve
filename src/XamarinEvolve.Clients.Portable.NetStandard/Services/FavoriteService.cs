using System;
using XamarinEvolve.DataObjects;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinEvolve.DataStore.Abstractions;
using FormsToolkit;
using System.Linq;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
	public class FavoriteService
	{
		Session sessionQueued;
        IDependencyService _locator;

        public FavoriteService(IDependencyService locator)
        {
            _locator = locator;
        }

        public FavoriteService() : this(new DependencyServiceWrapper())
		{
			MessagingService.Current.Subscribe(MessageKeys.LoggedIn, async (s) =>
				{
					MessagingService.Current.Unsubscribe(MessageKeys.LoggedIn);

					if (sessionQueued == null)
						return;

					await ToggleFavorite(sessionQueued);
				});
		}

		private bool _busy;

		public async Task<bool> ToggleFavorite(Session session)
		{
			if (!Settings.Current.IsLoggedIn)
			{
				sessionQueued = session;
				MessagingService.Current.SendMessage(MessageKeys.NavigateLogin);
				return false;
			}

			if (_busy)
			{
				return false;
			}

			_busy = true;
			sessionQueued = null;

			try
			{
                var actionWrapper = _locator.Get<IPlatformActionWrapper<Session>>();
				actionWrapper?.Before(session);

				var store = _locator.Get<IFavoriteStore>();
				var targetValue = !session.IsFavorite;
				session.IsFavorite = targetValue; //switch first so UI updates :)

				actionWrapper?.Success(session);

				if (!targetValue)
				{
                    _locator.Get<ILogger>().Track(EvolveLoggerKeys.FavoriteRemoved, "Title", session.Title);
				}

				// always clean up existing rows to be sure
				await store.RemoveBySessionIdAsync(session.Id).ConfigureAwait(false);

				if (targetValue)
				{
					_locator.Get<ILogger>().Track(EvolveLoggerKeys.FavoriteAdded, "Title", session.Title);
					await store.InsertAsync(new Favorite { SessionId = session.Id });
				}

				Settings.Current.LastFavoriteTime = Clock.Now;

				var dataHandler = _locator.Get<IPlatformSpecificDataHandler<Session>>();
				if (dataHandler != null)
				{
					await dataHandler.UpdateSingleEntity(session).ConfigureAwait(false);
				}

				session.IsFavorite = targetValue;
				MessagingService.Current.SendMessage(MessageKeys.SessionFavoriteToggled, session);
				return true;
			}
			catch (Exception e)
			{
				_locator.Get<ILogger>().Report(e, "ToggleFavoriteSession", session.Id, Severity.Error);
				return false;
			}
			finally
			{
				_busy = false;
			}
		}
	}
}

