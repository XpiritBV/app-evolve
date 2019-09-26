using System;
using System.Linq;
using System.Collections.Generic;
using FormsToolkit;
using MvvmHelpers;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using System.Windows.Input;
using System.Threading.Tasks;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
	public class SpeakersViewModel: ViewModelBase
	{
		public SpeakersViewModel(INavigation navigation) : base(navigation)
        {

		}

        protected override void UpdateCommandCanExecute()
        {
            loadSpeakersCommand?.ChangeCanExecute();
            forceRefreshCommand?.ChangeCanExecute();
        }

		public ObservableRangeCollection<Speaker> Speakers { get; } = new ObservableRangeCollection<Speaker>();

		#region Properties
		Speaker selectedSpeaker;
		public Speaker SelectedSpeaker
		{
			get { return selectedSpeaker; }
			set
			{
				selectedSpeaker = value;
				OnPropertyChanged();
				if (selectedSpeaker == null)
					return;

				MessagingService.Current.SendMessage(MessageKeys.NavigateToSpeaker, selectedSpeaker);

				SelectedSpeaker = null;
			}
		}

		public bool NoSpeakers
		{
			get { return (!IsBusy) && (Speakers.Count == 0); }
		}
		#endregion

		#region Sorting

		void SortSpeakers(IEnumerable<Speaker> speakers)
		{
			var speakersSorted = from speaker in speakers
								 orderby speaker.FullName
								 select speaker;

			Speakers.ReplaceRange(speakersSorted);
		}

		#endregion

		#region Commands

		Command forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
            forceRefreshCommand ?? (forceRefreshCommand = new Command(() => ExecuteForceRefreshCommandAsync().IgnoreResult(ShowError), () => !IsBusy));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadSpeakersAsync(true);
		}

		Command loadSpeakersCommand;
		public ICommand LoadSpeakersCommand =>
            loadSpeakersCommand ?? (loadSpeakersCommand = new Command((f) => ExecuteLoadSpeakersAsync((bool) f).IgnoreResult(ShowError), (arg) => !IsBusy));

		async Task<bool> ExecuteLoadSpeakersAsync(bool force = false)
		{
			if (IsBusy)
				return false;

			try
			{
				IsBusy = true;
				OnPropertyChanged("NoSpeakers");
	

				force = force || NextForceRefresh <= Clock.Now;
				if (force)
				{
					NextForceRefresh = Clock.Now.AddMinutes(AppBehavior.RefreshIntervalInMinutes);
				}

				StartTimer();
				Dictionary<string, string> param = new Dictionary<string, string>();
				param.Add("Expand", "Sessions");
                var speakers = await StoreManager.SpeakerStore.GetItemsAsync(force, param);
                DumpTiming($"Speakers: GetItemsAsync({force})");

                SortSpeakers(speakers);
                DumpTiming("SortSpeakers");
				OnPropertyChanged("NoSpeakers");
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadSpeakersAsync");
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

