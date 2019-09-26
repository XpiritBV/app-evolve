using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.Clients.UI
{
	public partial class EventsPage : BasePage
	{
		public override AppPage PageType => AppPage.Events;

        EventsViewModel vm;
        EventsViewModel ViewModel => vm ?? (vm = BindingContext as EventsViewModel);

		public EventsPage()
        {
			BindingContext = new EventsViewModel(Navigation);
			InitializeComponent();

            

            ListViewEvents.ItemTapped += (sender, e) => ListViewEvents.SelectedItem = null;
            ListViewEvents.ItemSelected += async (sender, e) => 
                {
                    var ev = ListViewEvents.SelectedItem as FeaturedEvent;
                    if(ev == null)
                        return;
                    
                    var eventDetails = new EventDetailsPage();

                    eventDetails.Event = ev;
                    await NavigationService.PushAsync(Navigation, eventDetails);

                    ListViewEvents.SelectedItem = null;
                };
        }
            
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel.Events.Count == 0)
                ViewModel.LoadEventsCommand.Execute(false);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }
}

