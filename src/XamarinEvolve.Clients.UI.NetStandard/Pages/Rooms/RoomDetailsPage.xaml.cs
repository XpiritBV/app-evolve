using System;
using System.Linq;
using System.Reflection;
using XamarinEvolve.Utils.Helpers;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.UI
{
    public partial class RoomDetailsPage : BasePage
    {
		public override AppPage PageType => AppPage.RoomDetails;

		RoomDetailsViewModel vm;

		public RoomDetailsPage(Room room)
        {
			ItemId = room?.Id;

            InitializeComponent();
			BindingContext = vm = new RoomDetailsViewModel(room);

			SetupSessionNavigation();
			LoadFloorPlan(room);
        }

		private void LoadFloorPlan(Room room)
		{
			var baseUrl = DependencyService.Get<IBaseUrl>();

			var floorPlansHtml = "";
			if (room != null)
			{
				floorPlansHtml = ResourceLoader.GetEmbeddedResourceString(Assembly.Load(new AssemblyName("XamarinEvolve.Clients.Portable.NetStandard")), "Floorplan.html");
				floorPlansHtml = floorPlansHtml
					.Replace("{{floor}}", room.FloorLevel?.ToString() ?? "1")
					.Replace("{{xpos}}", room.XCoordinate?.ToString() ?? "0")
					.Replace("{{ypos}}", room.YCoordinate?.ToString() ?? "0")
					.Replace("{{markercolor}}", "#FFF100");
			}
			else
			{
				floorPlansHtml = ResourceLoader.GetEmbeddedResourceString(Assembly.Load(new AssemblyName("XamarinEvolve.Clients.Portable.NetStandard")), "Floorplans.html");
			}

			FloorMapsView.Source = new HtmlWebViewSource
			{
				BaseUrl = baseUrl.Get(),
				Html = floorPlansHtml
			};
	
		}

		private void SetupSessionNavigation()
		{
			ListViewSessions.ItemSelected += async (sender, e) =>
			{
				var session = ListViewSessions.SelectedItem as Session;
				if (session == null)
					return;

				var sessionDetails = new SessionDetailsPage(session);

				await NavigationService.PushAsync(Navigation, sessionDetails);

				ListViewSessions.SelectedItem = null;
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (!vm.Sessions.Any())
			{
				vm.LoadSessionsCommand.Execute(null);
			}
		}

		async void Handle_Zoom_FloorPlan(object sender, System.EventArgs e)
		{
			await NavigationService.PushAsync(Navigation, new FloorMapPage(vm.Room), false);
		}
	}
}
