using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using System;
using FormsToolkit;
using XamarinEvolve.Utils;
using System.Threading.Tasks;

namespace XamarinEvolve.Clients.UI
{
    public partial class FilterSessionsPage : BasePage
	{
		public override AppPage PageType => AppPage.Filter;

        FilterSessionsViewModel vm;

        public FilterSessionsPage()
        {
			BindingContext = vm = new FilterSessionsViewModel(Navigation);

			InitializeComponent();

            //if (Device.RuntimePlatform != Device.iOS)
            //{
            //    ToolbarDone.Icon = "toolbar_close.png";
            //}
            
            ToolbarDone.Command = new Command(() => ApplyFilters(true).IgnoreResult(ShowError));

            LoadCategories().IgnoreResult(ShowError);
        }

        protected void ShowError(Exception ex)
        {
            DependencyService.Get<ILogger>().Report(ex);
            MessagingService.Current.SendMessage(MessageKeys.Error, ex);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ApplyFilters().IgnoreResult(ShowError);
        }

		async Task ApplyFilters(bool navigateExplicit = false)
        {
            vm.SaveCommand.Execute(default(object));
			if (navigateExplicit)
			{
				await Navigation.PopModalAsync();
			}
            if (Device.RuntimePlatform == Device.Android)
            {
                MessagingService.Current.SendMessage("filter_changed");
            }
        }

        async Task LoadCategories()
        {
            await vm.LoadCategoriesAsync();

            TableSectionFilters.Add(new CategoryCell
            {
                BindingContext = vm.ShowPastSessions
            });

            TableSectionFilters.Add(new CategoryCell
            {
                BindingContext = vm.ShowFavoritesOnly
            });

            var allCell = new CategoryCell
            {
                BindingContext = vm.AllCategories
            };

            TableSectionCategories.Add(allCell);

            foreach (var item in vm.Categories)
            {
                TableSectionCategories.Add(new CategoryCell
                {
                    BindingContext = item
                });
            }

            //if end of conference
            if (Clock.Now > EventInfo.EndOfConference)
            {
                vm.ShowPastSessions.IsEnabled = true;
            }
        }
    }
}

