using System.Linq;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.Clients.UI
{
	public partial class SpeakersPage : BasePage
	{
		public override AppPage PageType => AppPage.Speakers;

		SpeakersViewModel vm;
		SpeakersViewModel ViewModel => vm ?? (vm = BindingContext as SpeakersViewModel);

		public SpeakersPage()
		{
			BindingContext = new SpeakersViewModel(Navigation);
			InitializeComponent();

            if (Device.RuntimePlatform == Device.Android)
            {
                ListViewSpeakers.Effects.Add(Effect.Resolve("Xpirit.ListViewSelectionOnTopEffect"));
            }

			
			ListViewSpeakers.ItemSelected += async (sender, e) =>
			{
                if (e.SelectedItem is Speaker speaker)
                {
                    await NavigationService.PushAsync(Navigation, new SpeakerDetailsPage(speaker));
                    ListViewSpeakers.SelectedItem = null;
                }
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

            if (!(ViewModel.Speakers?.Any() ?? false))
            {
                ViewModel.LoadSpeakersCommand.Execute(false);
            }
		}
	}
}
