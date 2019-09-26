using System.Linq;
using System.Threading.Tasks;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
	public class ScavengerHuntStore : BaseStore<ScavengerHunt>, IScavengerHuntStore
    {
		public ScavengerHuntStore() : base (new DependencyServiceWrapper())
		{ }

		public ScavengerHuntStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

		public override string Identifier => "ScavengerHunt";

		public async Task<ScavengerHunt> GetAppIndexScavengerHunt(string id)
		{
			await InitializeStore().ConfigureAwait(false);
			var hunts = await Table.Where(s => s.Id == id || s.RemoteId == id).ToListAsync();

			return hunts?.FirstOrDefault();
		}
	}
}
