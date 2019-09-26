using System;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using FormsToolkit;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Utils;
using System.Linq;

namespace XamarinEvolve.Clients.UI
{
    public partial class SessionsPage : BasePage
	{
		public override AppPage PageType => AppPage.Sessions;

        SessionsViewModel ViewModel => vm ?? (vm = BindingContext as SessionsViewModel);
        SessionsViewModel vm;
        bool showFavs, showPast, showAllCategories;
        string filteredCategories;
        ToolbarItem filterItem;
        string loggedIn;
        public SessionsPage()
        {
			BindingContext = vm = new SessionsViewModel(Navigation);
			
            loggedIn = Settings.Current.UserIdentifier;
			showFavs = Settings.Current.FavoritesOnly;
			showPast = Settings.Current.ShowPastSessions;
			showAllCategories = Settings.Current.ShowAllCategories;
			filteredCategories = Settings.Current.FilteredCategories;

			InitializeComponent();

           
            filterItem = new ToolbarItem
            {
                Text = "Filter"
            };

            if (Device.RuntimePlatform != Device.iOS)
                filterItem.IconImageSource = "toolbar_filter.png";

            filterItem.Command = new Command(async () => 
                {
                    if (vm.IsBusy)
                        return;
                    await NavigationService.PushModalAsync(Navigation, new EvolveNavigationPage(new FilterSessionsPage()));
                });

            ToolbarItems.Add(filterItem);

            ListViewSessions.ItemSelected += async (sender, e) => 
                {
                    var session = ListViewSessions.SelectedItem as Session;
                    if(session == null || session.Categories.FirstOrDefault().BadgeName=="na")
                        return;
                    
                    var sessionDetails = new SessionDetailsPage(session);

                    await NavigationService.PushAsync(Navigation, sessionDetails);
                    ListViewSessions.SelectedItem = null;
                };
        }

        void ListViewTapped (object sender, ItemTappedEventArgs e)
        {
            var list = sender as ListView;
            if (list == null)
                return;
            list.SelectedItem = null;
        }
       
        protected override void OnAppearing()
        {
            base.OnAppearing();

            ListViewSessions.ItemTapped += ListViewTapped;

            if (Device.RuntimePlatform == Device.Android)
                MessagingService.Current.Subscribe("filter_changed", (d) => UpdatePage());
            
            UpdatePage();
        }

        void UpdatePage()
        {
            Title = Settings.Current.FavoritesOnly ? "Favorite Sessions" : "Sessions";

            if (!Settings.Current.ShowAllCategories)
            {
                Title += " (filtered)";
            }

            bool forceRefresh = (Clock.Now > (ViewModel?.NextForceRefresh ?? Clock.Now)) ||
                loggedIn != Settings.Current.UserIdentifier;

            loggedIn = Settings.Current.UserIdentifier;
            //Load if none, or if refresh interval minutes has gone by
            if ((ViewModel?.Sessions?.Count ?? 0) == 0 || forceRefresh)
            {
                ViewModel?.LoadSessionsCommand?.Execute(forceRefresh);
            }
            else if (showFavs != Settings.Current.FavoritesOnly ||
                    showPast != Settings.Current.ShowPastSessions ||
                    showAllCategories != Settings.Current.ShowAllCategories ||
                    filteredCategories != Settings.Current.FilteredCategories)
            {
                showFavs = Settings.Current.FavoritesOnly;
                showPast = Settings.Current.ShowPastSessions;
                showAllCategories = Settings.Current.ShowAllCategories;
                filteredCategories = Settings.Current.FilteredCategories;
                ViewModel?.FilterSessionsCommand?.Execute(null);
            }
        }

        protected override void OnDisappearing()

        {
            base.OnDisappearing();
            ListViewSessions.ItemTapped -= ListViewTapped;
            if (Device.RuntimePlatform == Device.Android)
                MessagingService.Current.Unsubscribe("filter_changed");
        }

        public void OnResume()
        {
            UpdatePage();
        }
    }

	public class SessionDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate SessionTemplate { get; set; }
		public DataTemplate NonSessionTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var session = item as Session;
			if(session.Categories.FirstOrDefault().BadgeName!= "na")
			{
				return SessionTemplate;
			}
			else
			{
				return NonSessionTemplate;
			}
		}
	}
}

