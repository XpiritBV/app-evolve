using XamarinEvolve.DataStore.Abstractions;
using System.Threading.Tasks;
using System.Collections.Generic;
using XamarinEvolve.DataObjects;
using System.Linq;
using XamarinEvolve.Utils;
using System.Diagnostics;
using System;

namespace XamarinEvolve.DataStore.Azure
{
	public class SessionStore : BaseStore<Session>, ISessionStore
    {
		public SessionStore() : base(new DependencyServiceWrapper())
		{ }

		public SessionStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

        public override async Task<IEnumerable<Session>> GetItemsAsync(bool forceRefresh = false, Dictionary<string, string> param = null)
        {
            await InitializeStore().ConfigureAwait (false);

            if (forceRefresh)
            {
				param.Add("expand", "Speakers");
				await PullLatestAsync(param).ConfigureAwait(false);
            }

            var sessions = await Table.OrderBy(s => s.StartTime).ToListAsync().ConfigureAwait(false);
			var success = AnnotateFavorites(sessions).ConfigureAwait(false).GetAwaiter().GetResult();

			var dataShare = Locator.Get<IPlatformSpecificDataHandler<Session>>();
			if (dataShare != null)
			{
                dataShare.UpdateMultipleEntities(sessions).IgnoreResultBackgroundThread();
			}

            return sessions;
        }

		private async Task<bool> AnnotateFavorites(IEnumerable<Session> sessions)
		{
			try
			{
				var favStore = Locator.Get<IFavoriteStore>();
				var favs = await favStore.GetItemsAsync(false).ConfigureAwait(false);

				foreach (var session in sessions)
				{
					session.IsFavorite = favs.Any(f => f.SessionId == session.Id);
				}
			}
			catch(Exception e)
			{
				Debug.WriteLine(e.Message);
				return false;
			}
			return true;
		}

        public async Task<IEnumerable<Session>> GetSpeakerSessionsAsync(string speakerId)
        {
			await InitializeStore().ConfigureAwait (false);
			var sessions = await Table.Where(s => s.SpeakerIdString.Contains(speakerId))
									  .ToListAsync()
									  .ConfigureAwait(false);

			await AnnotateFavorites(sessions).ConfigureAwait(false);

			return sessions
				.OrderBy(session => session.StartTimeOrderBy)
				.ThenBy(session => session.Title);
        }

		async Task<IEnumerable<Session>> ISessionStore.GetNextSessions(int maxNumberOfSessionsToReturn)
		{
			var referenceDate = Clock.Now.AddMinutes(-30); //about to start in next 30

			var favStore = Locator.Get<IFavoriteStore>();
			var favorites = await favStore.GetItemsAsync(false).ConfigureAwait(false);
			var favoriteIds = favorites.Select(f => f.SessionId).ToList();
			if (!favoriteIds.Any())
			{
				return null;
			}

			var sessions = await Table.Where(s => favoriteIds.Contains(s.Id) && s.StartTime != null && s.StartTime > referenceDate)
			                          .OrderBy(s => s.StartTime)
			                          .Take(maxNumberOfSessionsToReturn)
									  .ToListAsync()
									  .ConfigureAwait(false);

			foreach (var s in sessions)
			{
				s.IsFavorite = true;
			}

			return sessions.Any() ? sessions : null;
		}

		public async Task<Session> GetAppIndexSession (string id)
        {
            await InitializeStore ().ConfigureAwait (false);
            var sessions = await Table.Where (s => s.Id == id || s.RemoteId == id).ToListAsync();

            if (sessions == null || sessions.Count == 0)
                return null;
            
            return sessions [0];
        }

        public async Task<IEnumerable<Session>> GetRoomSessions(string roomId)
        {
			var referenceDate = Clock.Now.AddMinutes(-30); //about to start in next 30

			await InitializeStore().ConfigureAwait(false);

			var sessions = await Table.Where(s => s.RoomIdString == roomId && (Settings.Current.ShowPastSessions || (s.StartTime != null && s.StartTime > referenceDate)))
			                          .ToListAsync()
			                          .ConfigureAwait(false);

			await AnnotateFavorites(sessions).ConfigureAwait(false);

			return sessions.OrderBy(s => s.StartTimeOrderBy);
		}

        public override string Identifier => "Session";
    }
}

