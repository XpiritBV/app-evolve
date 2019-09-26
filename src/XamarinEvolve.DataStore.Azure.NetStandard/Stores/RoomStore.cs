using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
    public class RoomStore : BaseStore<Room>, IRoomStore
	{
        public RoomStore() : base(new DependencyServiceWrapper())
		{ }

		public RoomStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

		public override string Identifier => "Room";
	}
}
