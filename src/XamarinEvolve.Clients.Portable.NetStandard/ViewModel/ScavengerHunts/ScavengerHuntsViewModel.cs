using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FormsToolkit;
using MvvmHelpers;
using Xamarin.Forms;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
	public class ScavengerHuntsViewModel : ViewModelBase
    {
        public ScavengerHuntsViewModel()
        {
			Title = "Treasure Hunts";
        }

		protected override void UpdateCommandCanExecute()
		{
			forceRefreshCommand?.ChangeCanExecute();
			loadScavengerHuntsCommand?.ChangeCanExecute();
		}

		public ObservableRangeCollection<ScavengerHuntViewModel> ScavengerHunts { get; } = new ObservableRangeCollection<ScavengerHuntViewModel>();

		public bool ShowIntro => true;
		public string ScavengerHuntIntroText => $"Embedded in this app, and using Microsoft Cognitive Services, we have put together a treasure hunt for you. Throughout the event there will be hidden items to find and to recognise using your phones; 8 in total. For the person who is the quickest to find all 8 there will be a prize on offer of a 3D scan of yourself which is being offered via 3Dmij. The hunt will be run once per day. Use your camera to take a picture of the objects you find to unlock them through Custom Vision. With each failed attempt, the score drops. You have a maximum of {AppBehavior.MaxTotalAttemptsPerDay} attempts per day.";

		bool noHuntsFound;
		public bool NoHuntsFound
		{
            get => noHuntsFound;
			set => SetProperty(ref noHuntsFound, value);
		}

		public string NoHuntsText => $"{Title} will be revealed at {EventInfo.EventName}. Check back soon.";

		#region Commands

		Command forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
			forceRefreshCommand ?? (forceRefreshCommand = new Command(() => ExecuteForceRefreshCommandAsync().IgnoreResult(ShowError), () => !IsBusy));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadScavengerHunts(true);
		}

        Command loadScavengerHuntsCommand;
        public ICommand LoadScavengerHuntsCommand =>
			loadScavengerHuntsCommand ?? (loadScavengerHuntsCommand = new Command<bool>((f) => ExecuteLoadScavengerHunts().IgnoreResult(ShowError), (arg) => !IsBusy));

		public Task<bool> IsRegistered()
		{
			return StoreManager.AttendeeStore.IsRegistered();
		}

		async Task<bool> ExecuteLoadScavengerHunts(bool force = false)
		{
			if (IsBusy)
				return false;

			try
			{
				IsBusy = true;
				NoHuntsFound = false;

				force = force || NextForceRefresh <= Clock.Now;
				if (force)
				{
					NextForceRefresh = Clock.Now.AddMinutes(AppBehavior.RefreshIntervalInMinutes);
				}

                var hunts = await StoreManager.ScavengerHuntStore.GetItemsAsync(force).ConfigureAwait(false);
				var finalHunts = hunts.Select(h => new ScavengerHuntViewModel(h)).ToList();

				Device.BeginInvokeOnMainThread(() =>
				{
					ScavengerHunts.ReplaceRange(finalHunts.OrderBy(h => h.OpenFrom));
                    NoHuntsFound = !ScavengerHunts.Any();
				});
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadScavengerHunts");
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
