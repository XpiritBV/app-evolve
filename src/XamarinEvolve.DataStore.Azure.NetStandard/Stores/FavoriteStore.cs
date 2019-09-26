using System;
using System.Threading.Tasks;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
    public class FavoriteStore : BaseStore<Favorite>, IFavoriteStore
    {
		public FavoriteStore() : base (new DependencyServiceWrapper())
		{ }

		public FavoriteStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

		public Task<bool> IsFavorite(string sessionId)
        {
            throw new NotImplementedException("We don't want N+1 queries");
        }

        public Task DropFavorites()
        {
            return Task.FromResult(true);
        }

		public async Task RemoveBySessionIdAsync(string id)
		{
			var items = await Table.Where(f => f.SessionId == id)
			                       .ToListAsync()
			                       .ConfigureAwait(false);

			foreach (var item in items)
			{
				await RemoveAsync(item).ConfigureAwait(false);
			}
		}

		public override string Identifier => "Favorite";
    }
}

