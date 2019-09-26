using System.Reflection;
using XamarinEvolve.Utils.Helpers;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.UI
{
	public partial class SponsorDetailsPage : BasePage
	{
		public override AppPage PageType => AppPage.Sponsor;

        SponsorDetailsViewModel ViewModel => vm ?? (vm = BindingContext as SponsorDetailsViewModel);
        SponsorDetailsViewModel vm;

        public SponsorDetailsPage()
        {
            InitializeComponent();           
        }

        public Sponsor Sponsor
        {
            get { return ViewModel.Sponsor; }
            set 
			{ 
				BindingContext = new SponsorDetailsViewModel(Navigation, value);
				ItemId = value?.Name;
			}
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            vm = null;

			ToolbarItems.Clear();
			if (ViewModel?.Sponsor?.XCoordinate != null && ViewModel?.Sponsor?.YCoordinate != null && ViewModel?.Sponsor?.FloorLevel != null)
			{
				ToolbarItems.Add(new ToolbarItem("Floormap", "toolbar_locate.png", () => Locate_Sponsor()));
			}

            var adjust = Device.RuntimePlatform != Device.Android ? 1 : -ViewModel.FollowItems.Count + 1;
            ListViewFollow.HeightRequest = (ViewModel.FollowItems.Count * ListViewFollow.RowHeight) - adjust;
        }

		async void Locate_Sponsor()
		{
			var dummyRoom = new Room 
			{ 
				XCoordinate = Sponsor.XCoordinate, 
				YCoordinate = Sponsor.YCoordinate, 
				FloorLevel = Sponsor.FloorLevel 
			};
			await NavigationService.PushAsync(Navigation, new FloorMapPage(dummyRoom, "#E81123"), false);
		}
	}
}

