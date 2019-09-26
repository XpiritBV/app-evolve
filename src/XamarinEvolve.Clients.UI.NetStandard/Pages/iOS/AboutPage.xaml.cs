using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.UI
{
    public partial class AboutPage : BasePage
	{
		public override AppPage PageType => AppPage.Information;

        AboutViewModel vm;
        IPushNotifications push;
        public AboutPage()
        {
            InitializeComponent();
            BindingContext = vm = new AboutViewModel();
            push = DependencyService.Get<IPushNotifications>();
            var adjust = Device.RuntimePlatform != Device.Android ? 1 : -vm.AboutItems.Count + 1;
            ListViewAbout.HeightRequest = (vm.AboutItems.Count * ListViewAbout.RowHeight) - adjust;
            ListViewAbout.ItemTapped += (sender, e) => ListViewAbout.SelectedItem = null;
            ListViewInfo.HeightRequest = (vm.InfoItems.Count * ListViewInfo.RowHeight) - adjust;

            ListViewAccount.HeightRequest = (vm.AccountItems.Count * ListViewAccount.RowHeight) - adjust;
			ListViewAccount.ItemSelected +=  (sender, e) =>
			{
				var item = ListViewAccount.SelectedItem as Portable.MenuItem;
                if (item == null)
                {
                    return;
                }
				
				ListViewAccount.SelectedItem = null;

                return;
			};

            ListViewAbout.ItemSelected += async (sender, e) => 
                {
                    if (ListViewAbout.SelectedItem == null)
                    {
                        return;
                    }

                    await NavigationService.PushAsync(Navigation, new SettingsPage());

                    ListViewAbout.SelectedItem = null;
                };

            ListViewPrivacy.ItemSelected += async (sender, e) => 
            {
				ListViewPrivacy.SelectedItem = null;

				var item = ListViewPrivacy.SelectedItem as Portable.MenuItem;
                if (item == null)
                {
                    return;
                }

				Page page = null;
				switch (item.Parameter)
				{
					case "privacy":
                        vm.LaunchBrowserCommand.Execute(AboutThisApp.PrivacyPolicyUrl);
						break;
					case "code-of-conduct":
						page = new CodeOfConductPage();
						break;
				}

                if (page == null)
                {
                    return;
                }
                
				await NavigationService.PushAsync(Navigation, page);
			};

            ListViewInfo.ItemSelected += async (sender, e) => 
                {
                    var item = ListViewInfo.SelectedItem as Portable.MenuItem;
                    if(item == null)
                        return;
                    Page page = null;
                    switch(item.Parameter)
                    {
                        case "evaluations":
                            page = new EvaluationsPage ();
                            break;
                        case "venue":
                            page = new VenuePage();
                            break;
                        case "wi-fi":
                            page = new WiFiInformationPage();
                            break;
                        case "sponsors":
                            page = new SponsorsPage();
                            break;
                        case "floor-maps":
                            App.Logger.TrackPage(AppPage.FloorMap.ToString());
                            page = new FloorMapPage(null);
                            break;
					}

                    if (page == null)
                    {
                        return;
                    }

                    if (Device.RuntimePlatform == Device.iOS && page is VenuePage)
                    {
                        await NavigationService.PushAsync(((Page)this.Parent.Parent).Navigation, page);
                    }
                    else
                    {
                        await NavigationService.PushAsync(Navigation, page);
                    }

                    ListViewInfo.SelectedItem = null;
                };
            isRegistered = push.IsRegistered;
        }

        bool isRegistered;
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!isRegistered && Settings.Current.AttemptedPush)
            {
                push.RegisterForNotifications();
            }
            isRegistered = push.IsRegistered;
            vm.UpdateItems();
        }

        public void OnResume()
        {
            OnAppearing();
        }
    }
}

