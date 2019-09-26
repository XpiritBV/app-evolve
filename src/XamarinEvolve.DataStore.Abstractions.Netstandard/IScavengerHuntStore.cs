using System;
using System.Threading.Tasks;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.DataStore.Abstractions
{
	public interface IScavengerHuntStore : IBaseStore<ScavengerHunt>
	{
		Task<ScavengerHunt> GetAppIndexScavengerHunt(string id);
	}
}
