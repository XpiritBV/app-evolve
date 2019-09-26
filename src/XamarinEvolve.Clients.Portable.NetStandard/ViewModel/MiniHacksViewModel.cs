using System;
using MvvmHelpers;
using XamarinEvolve.DataObjects;
using System.Windows.Input;
using System.Threading.Tasks;
using FormsToolkit;
using Xamarin.Forms;
using System.Linq;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
    public class MiniHacksViewModel : ViewModelBase
    {
        public MiniHacksViewModel()
        {
        }

        protected override void UpdateCommandCanExecute()
        {
            forceRefreshCommand?.ChangeCanExecute();
            loadMiniHacksCommand?.ChangeCanExecute();
        }

        public ObservableRangeCollection<MiniHack> MiniHacks { get; } = new ObservableRangeCollection<MiniHack>();

        bool noHacksFound;
        public bool NoHacksFound
        {
            get { return noHacksFound; }
            set { SetProperty(ref noHacksFound, value); }
        }

		public string NoHacksText => $"Mini-Hacks will be revealed at {EventInfo.EventName}. Check back soon.";

        #region Commands

        Command  forceRefreshCommand;
        public ICommand ForceRefreshCommand =>
            forceRefreshCommand ?? (forceRefreshCommand = new Command(() => ExecuteForceRefreshCommandAsync().IgnoreResult(ShowError), () => !IsBusy)); 

        async Task ExecuteForceRefreshCommandAsync()
        {
            await ExecuteLoadMiniHacksAsync(true);
        }

        Command loadMiniHacksCommand;
        public ICommand LoadMiniHacksCommand =>
            loadMiniHacksCommand ?? (loadMiniHacksCommand = new Command<bool>((f) => ExecuteLoadMiniHacksAsync().IgnoreResult(ShowError), (arg) => !IsBusy)); 

        async Task<bool> ExecuteLoadMiniHacksAsync(bool force = false)
        {
            if(IsBusy)
                return false;

            try 
            {
                IsBusy = true;
                NoHacksFound = false;

				force = force || NextForceRefresh <= Clock.Now;
				if (force)
				{
					NextForceRefresh = Clock.Now.AddMinutes(AppBehavior.RefreshIntervalInMinutes);
				}

				var hacks = await StoreManager.MiniHacksStore.GetItemsAsync(force).ConfigureAwait(false);
                var finalHacks = hacks.ToList ();

                foreach (var hack in finalHacks)
                {
                    hack.IsCompleted = Settings.Current.IsHackFinished(hack.Id);
                }

				Device.BeginInvokeOnMainThread(() =>
				{
					MiniHacks.ReplaceRange(finalHacks);
					NoHacksFound = !MiniHacks.Any();
				});
            } 
            catch (Exception ex) 
            {
                Logger.Report(ex, "Method", "ExecuteLoadMiniHacksAsync");
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

