using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.DataObjects;
using System.Threading.Tasks;
using System.Linq;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
    public class SpeakerStore : BaseStore<Speaker>, ISpeakerStore
    {
		public SpeakerStore() : base(new DependencyServiceWrapper())
		{ }

		public SpeakerStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

		public override string Identifier => "Speaker";

		public async Task<Speaker> GetAppIndexSpeaker(string id)
		{
			await InitializeStore().ConfigureAwait(false);
			var speakers = await Table.Where(s => s.Id == id || s.RemoteId == id).ToListAsync();

			if (speakers == null || !speakers.Any())
				return null;

			return speakers[0];
		}

	}
}

