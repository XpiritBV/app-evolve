using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Utils;
using XamarinEvolve.Utils.Helpers;
using System.Reflection;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.Clients.UI
{
    public partial class FloorMapPage : BasePage
	{
		public override AppPage PageType => AppPage.FloorMap;

        public FloorMapPage (Room room, string markerColor = "#FFF100")
        {
            InitializeComponent ();

		
			var baseUrl = DependencyService.Get<IBaseUrl>();

			var floorPlansHtml = "";
			if (room != null)
			{
				floorPlansHtml = ResourceLoader.GetEmbeddedResourceString(Assembly.Load(new AssemblyName("XamarinEvolve.Clients.Portable.NetStandard")), "Floorplan.html");
				floorPlansHtml = floorPlansHtml
					.Replace("{{floor}}", room.FloorLevel?.ToString() ?? "1")
					.Replace("{{xpos}}", room.XCoordinate?.ToString() ?? "0")
					.Replace("{{ypos}}", room.YCoordinate?.ToString() ?? "0")
					.Replace("{{markercolor}}", markerColor);
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

		void Handle_Navigated(object sender, Xamarin.Forms.WebNavigatedEventArgs e)
		{
			ProgressBar.IsRunning = false;
			ProgressBar.IsVisible = false;
		}
	}
}

