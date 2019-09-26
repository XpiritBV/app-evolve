using System;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
    public class EventStore : BaseStore<FeaturedEvent>, IEventStore
    {
		public EventStore() : base (new DependencyServiceWrapper())
		{ }

		public EventStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

		public override string Identifier => "FeaturedEvent";
    }
}

