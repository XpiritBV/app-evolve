using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
	public class SponsorStore : BaseStore<Sponsor>, ISponsorStore
    {
		public SponsorStore() : base(new DependencyServiceWrapper())
		{ }

		public SponsorStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

		public override string Identifier => "Sponsor";
    }
}

