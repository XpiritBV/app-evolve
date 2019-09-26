using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using System.Collections.Generic;
using System.Windows.Input;

namespace XamarinEvolve.Clients.Portable
{
    public class FilterSessionsViewModel : ViewModelBase
    {
        public FilterSessionsViewModel(INavigation navigation)
            : base(navigation)
        {
            AllCategories = new Category
            {
                Name = "All",
                IsEnabled = true,
                IsFiltered = Settings.ShowAllCategories
            };

            AllCategories.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsFiltered")
                    SetShowAllCategories(AllCategories.IsFiltered);
            };

            ShowFavoritesOnly = new Category
            {
                Name = "Show Favorites Only",
                IsEnabled = true,
                IsFiltered = Settings.FavoritesOnly,
                ShortName = "Show Favorites Only"
            };

            ShowPastSessions = new Category
            {
                Name = "Show Past Sessions",
                IsEnabled = true,
                IsFiltered = Settings.ShowPastSessions,
                ShortName = "Show Past Sessions"
            };
        }

        protected override void UpdateCommandCanExecute()
        {
            _saveCommand?.ChangeCanExecute();
        }

        public Category AllCategories { get; }

        public Category ShowFavoritesOnly { get; }

		public Category ShowPastSessions { get; }

		public List<Category> Categories { get; } = new List<Category>();

        private Command _saveCommand;
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new Command(Save, () => !IsBusy));

        private void SetShowAllCategories(bool showAll)
        {
			// first save changes to individual filters
			SaveIndividualCategories();
            Settings.ShowAllCategories = showAll;
            foreach(var category in Categories)
            {
                category.IsEnabled = !Settings.ShowAllCategories;
                category.IsFiltered = Settings.ShowAllCategories || Settings.FilteredCategories.Contains(category.Name);
            }
        }

        public async Task LoadCategoriesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                Categories.Clear();
                var items = await StoreManager.CategoryStore.GetItemsAsync().ConfigureAwait(false);

                if (!items.Any())
                    items = await StoreManager.CategoryStore.GetItemsAsync(true).ConfigureAwait(false);

                foreach (var category in items.OrderBy(c => c.Name))
                {
                    category.IsFiltered = Settings.ShowAllCategories || Settings.FilteredCategories.Contains(category.Name);
                    category.IsEnabled = !Settings.ShowAllCategories;
					if(category.Name!="na")
						 Categories.Add(category);
                }

                SaveIndividualCategories();
            }
			catch (Exception e)
			{
				Logger.Report(e);
			}
			finally
            {
                IsBusy = false;
            }
        }

        private void Save()
        {
			Settings.ShowPastSessions = ShowPastSessions.IsFiltered;
			Settings.FavoritesOnly = ShowFavoritesOnly.IsFiltered;
            Settings.ShowAllCategories = AllCategories.IsFiltered;
            SaveIndividualCategories();
		}
               
        private void SaveIndividualCategories()
        {
			if (!Settings.ShowAllCategories)
			{
				Settings.FilteredCategories = string.Join("|", Categories?.Where(c => c.IsFiltered).Select(c => c.Name));
			}
        }
    }
}

