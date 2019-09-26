using System.Threading.Tasks;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;

namespace XamarinEvolve.DataStore.Mock
{
	public class ScavengerHuntStore : BaseStore<ScavengerHunt>, IScavengerHuntStore
	{
		public Task<ScavengerHunt> GetAppIndexScavengerHunt(string id)
		{
			return GetItemAsync(id);
		}
	}
}
