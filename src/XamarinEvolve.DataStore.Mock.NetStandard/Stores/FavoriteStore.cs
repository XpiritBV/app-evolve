using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Mock
{
    public class FavoriteStore : BaseStore<Favorite>, IFavoriteStore
    {
		public FavoriteStore()
		{

		}

		public FavoriteStore(IDependencyService locator) : base (locator)
        {
            
        }

        public Task<bool> IsFavorite(string sessionId)
        {
            return Task.FromResult(Settings.IsFavorite(sessionId));
        }

		public async override Task<IEnumerable<Favorite>> GetItemsAsync(bool forceRefresh = false, Dictionary<string, string> param = null)
		{
            var sessionStore = Locator.Get<ISessionStore>();
            var sessions = await sessionStore.GetItemsAsync().ConfigureAwait(false);

            List<Favorite> favs = new List<Favorite>();
            var i = 0;
            foreach (var session in sessions)
            {
                if (await IsFavorite(session.Id).ConfigureAwait(false))
                {
                    var fav = new Favorite { Id = i++.ToString(), SessionId = session.Id };
                    favs.Add(fav);
                }
            }

            return favs;
		}

		public override Task<bool> InsertAsync(Favorite item)
        {
            Settings.SetFavorite(item.SessionId, true);
            return Task.FromResult(true);
        }

        public override Task<bool> RemoveAsync(Favorite item)
        {
            Settings.SetFavorite(item.SessionId, false);
            return Task.FromResult(true);
        }

        public async Task DropFavorites()
        {
            await Settings.ClearFavorites();
        }

		public Task RemoveBySessionIdAsync(string id)
		{
			Settings.SetFavorite(id, false);
			return Task.FromResult(true);
		}
	}
}

