using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FormsToolkit;
using MvvmHelpers;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using System.Collections.Generic;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
    public class SponsorsViewModel : ViewModelBase
    {
        public SponsorsViewModel(INavigation navigation) : base(navigation)
        {

        }

        protected override void UpdateCommandCanExecute()
        {
            loadSponsorsCommand?.ChangeCanExecute();
            forceRefreshCommand?.ChangeCanExecute();
        }

		public ObservableRangeCollection<Grouping<string, Sponsor>> SponsorsGrouped { get; } = new ObservableRangeCollection<Grouping<string, Sponsor>>();

        #region Properties
        Sponsor selectedSponsor;
        public Sponsor SelectedSponsor
        {
            get { return selectedSponsor; }
            set
            {
                selectedSponsor = value;
                OnPropertyChanged();
                if (selectedSponsor == null)
                    return;
                 
                MessagingService.Current.SendMessage(MessageKeys.NavigateToSponsor, selectedSponsor);

                SelectedSponsor = null;
            }
        }
		     
        #endregion

        #region Sorting

        void SortSponsors(IEnumerable<Sponsor> sponsors)
        {
            var groups = from sponsor in sponsors
				orderby sponsor.SponsorLevel?.Rank ?? 9999
				group sponsor by sponsor.SponsorLevel?.Name ?? "Sponsor"
                into sponsorGroup
                select new Grouping<string, Sponsor>(sponsorGroup.Key, sponsorGroup.OrderBy(s => s.Rank)); 
            
            SponsorsGrouped.ReplaceRange(groups);
        }

        #endregion


        #region Commands

        Command  forceRefreshCommand;
        public ICommand ForceRefreshCommand =>
            forceRefreshCommand ?? (forceRefreshCommand = new Command(() => ExecuteForceRefreshCommandAsync().IgnoreResult(ShowError), () => !IsBusy)); 

        Task ExecuteForceRefreshCommandAsync()
        {
            return ExecuteLoadSponsorsAsync(true);
        }

        Command loadSponsorsCommand;
        public ICommand LoadSponsorsCommand =>
            loadSponsorsCommand ?? (loadSponsorsCommand = new Command((f) => ExecuteLoadSponsorsAsync().IgnoreResult(ShowError), (o) => !IsBusy)); 

        async Task<bool> ExecuteLoadSponsorsAsync(bool force = false)
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

				var sponsors = await StoreManager.SponsorStore.GetItemsAsync(force);
               
                SortSponsors(sponsors);

            } 
            catch (Exception ex) 
            {
                Logger.Report(ex, "Method", "ExecuteLoadSponsorsAsync");
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

