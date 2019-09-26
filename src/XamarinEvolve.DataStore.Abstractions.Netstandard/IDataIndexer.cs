using System.Threading.Tasks;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.DataStore.Abstractions
{
    public interface IDataIndexer
    {
        bool IsSupported();
        Task RegisterSession(Session item);
        Task RegisterSpeaker(Speaker item);
        Task RegisterMiniHack(MiniHack item);
	}
}
