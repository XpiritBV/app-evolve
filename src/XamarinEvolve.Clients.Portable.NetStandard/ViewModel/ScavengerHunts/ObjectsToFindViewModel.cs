using System.Linq;
using MvvmHelpers;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.Clients.Portable
{
    public class ObjectsToFindViewModel: ViewModelBase
    {
        public ObjectsToFindViewModel(ScavengerHuntViewModel hunt)
        {
            ScavengerHunt = hunt;
        }

        private ScavengerHuntViewModel scavengerHunt;
        public ScavengerHuntViewModel ScavengerHunt
        {
            get => scavengerHunt;
            set => SetProperty(ref scavengerHunt, value);
        }
    }
}
