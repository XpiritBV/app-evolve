using System.Collections.Generic;
using System.Threading.Tasks;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;

namespace XamarinEvolve.DataStore.Mock
{
    public class RoomStore : BaseStore<Room>, IRoomStore
    {
        public static readonly Room[] Rooms = {
            new Room {Id="1", Name = "Fossy Salon", FloorLevel=1, XCoordinate=100, YCoordinate=100},
            new Room {Id="2", Name = "Crick Salon", FloorLevel=1, XCoordinate=100, YCoordinate=100},
            new Room {Id="2", Name = "Franklin Salon", FloorLevel=1, XCoordinate=100, YCoordinate=100},
            new Room {Id="2", Name = "Goodall Salon", FloorLevel=1, XCoordinate=100, YCoordinate=100},
            new Room {Id="2", Name = "Linnaeus Salon", FloorLevel=1, XCoordinate=100, YCoordinate=100},
            new Room {Id="2", Name = "Watson Salon", FloorLevel=1, XCoordinate=100, YCoordinate=100},
        };

        public override Task<IEnumerable<Room>> GetItemsAsync(bool forceRefresh = false, Dictionary<string, string> param = null)
        {
            return Task.FromResult(Rooms as IEnumerable<Room>);
        }
    }
}
