using System;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using MvvmHelpers;
using FormsToolkit;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
    public class EventsViewModel : ViewModelBase
    {
        public EventsViewModel(INavigation navigation) : base(navigation)
        {
            Title = "Events";
        }

        protected override void UpdateCommandCanExecute()
        {
            loadEventsCommand?.ChangeCanExecute();
            forceRefreshCommand?.ChangeCanExecute();
        }

        public ObservableRangeCollection<FeaturedEvent> Events { get; } = new ObservableRangeCollection<FeaturedEvent>();
        public ObservableRangeCollection<Grouping<string, FeaturedEvent>> EventsGrouped { get; } = new ObservableRangeCollection<Grouping<string, FeaturedEvent>>();

        #region Properties
        FeaturedEvent selectedEvent;
        public FeaturedEvent SelectedEvent
        {
            get { return selectedEvent; }
            set
            {
                selectedEvent = value;
                OnPropertyChanged();
                if (selectedEvent == null)
                    return;

                MessagingService.Current.SendMessage(MessageKeys.NavigateToEvent, selectedEvent);

                SelectedEvent = null;
            }
        }
             
        #endregion

        #region Sorting

        void SortEvents()
        {
            EventsGrouped.ReplaceRange(Events.GroupByDate());
        }

        #endregion


        #region Commands

        Command  forceRefreshCommand;
        public ICommand ForceRefreshCommand =>
            forceRefreshCommand ?? (forceRefreshCommand = new Command(() => ExecuteForceRefreshCommandAsync().IgnoreResult(ShowError), () => !IsBusy)); 

        async Task ExecuteForceRefreshCommandAsync()
        {
            await ExecuteLoadEventsAsync(true);
        }

        Command loadEventsCommand;
        public ICommand LoadEventsCommand =>
            loadEventsCommand ?? (loadEventsCommand = new Command<bool>((f) => ExecuteLoadEventsAsync().IgnoreResult(ShowError), (arg) => !IsBusy)); 

        async Task<bool> ExecuteLoadEventsAsync(bool force = false)
        {
            if(IsBusy)
                return false;

            try 
            {
                IsBusy = true;

				force = force || NextForceRefresh <= Clock.Now;
				if (force)
				{
					NextForceRefresh = Clock.Now.AddMinutes(AppBehavior.RefreshIntervalInMinutes);
				}

				Events.ReplaceRange(await StoreManager.EventStore.GetItemsAsync(force));

				Title = "Events (" + Events.Count(e => e.StartTime.HasValue && e.StartTime.Value.ToUniversalTime() > Clock.Now) + ")";

                SortEvents();
            } 
            catch (Exception ex) 
            {
                Logger.Report(ex, "Method", "ExecuteLoadEventsAsync");
                MessagingService.Current.SendMessage(MessageKeys.Error, ex);
            }
            finally
            {
                IsBusy = false;
            }

            return true;
        }
        #endregion
    }
}

