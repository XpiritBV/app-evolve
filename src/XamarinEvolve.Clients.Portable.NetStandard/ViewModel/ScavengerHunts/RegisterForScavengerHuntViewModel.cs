using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FormsToolkit;
using Plugin.Connectivity;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
	public class RegisterForScavengerHuntViewModel : ViewModelBase
	{
		public RegisterForScavengerHuntViewModel()
		{
			MessagingService.Current.Subscribe(MessageKeys.LoggedIn, async (s) =>
			{
				MessagingService.Current.Unsubscribe(MessageKeys.LoggedIn);

				if (!queued)
					return;
				
				await SubmitScavengerHuntRegistration();
			});
		}

		protected override void UpdateCommandCanExecute()
		{
			SubmitRegistrationCommand?.ChangeCanExecute();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MessagingService.Current.Unsubscribe(MessageKeys.LoggedIn);
			}
		}

		bool queued = false;

		public string RegisterIntroText => "In order to participate in our Treasure Hunt, we need to know how to contact you in case you win a prize. Please enter your name and email address below. We use this data solely for contacting you about Treasure Hunts.";

		private string _fullName;
		public string FullName
		{
			get => _fullName;
			set
			{
				SetProperty(ref _fullName, value);
				OnPropertyChanged(nameof(IsValid));
				submitRegistrationCommand?.ChangeCanExecute();
			} 
		}

		private string _emailAddress;
		public string EmailAddress
		{
			get => _emailAddress;
			set
			{
				SetProperty(ref _emailAddress, value);
				OnPropertyChanged(nameof(IsValid));
				submitRegistrationCommand?.ChangeCanExecute();
			}
		}

		private Command submitRegistrationCommand;
		public Command SubmitRegistrationCommand => submitRegistrationCommand ?? (submitRegistrationCommand = new Command(() => SubmitScavengerHuntRegistration().IgnoreResult(ShowError), () => !IsBusy));

		private Command cancelRegistrationCommand;
		public Command CancelRegistrationCommand => cancelRegistrationCommand ?? (cancelRegistrationCommand = new Command(() => MessagingService.Current.SendMessage("user_registration_canceled")));

		public bool IsValid => !string.IsNullOrWhiteSpace(FullName) && (Regex.IsMatch(EmailAddress?.Trim(), AppBehavior.EmailRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));

		private async Task SubmitScavengerHuntRegistration()
		{
			if (!IsValid)
			{
				MessagingUtils.SendAlert("Oops", $"Please enter both your full name and a valid email address to register.");
				return;
			}
			if (IsBusy)
			{
				return;
			}

			queued = false;

			if (!CrossConnectivity.Current.IsConnected)
			{
				MessagingUtils.SendAlert("Offline", "You are currently offline. Please go online in order to register.");
				return;
			}

			if (!Settings.Current.IsLoggedIn)
			{
				queued = true;
				MessagingService.Current.SendMessage(MessageKeys.NavigateLogin);
				return;
			}

			try
			{
				IsBusy = true;

				var data = new Attendee
				{
					Name = FullName.Trim(),
					Email = EmailAddress.Trim()
				};

				await StoreManager.AttendeeStore.SubmitRegistration(data);

				MessagingUtils.SendAlert("Thanks!", "Thanks for registering. Enjoy our treasure hunt :)");

				MessagingService.Current.SendMessage("user_registered");
			}
			catch (Exception ex)
			{
				Logger.Report(ex);
				MessagingUtils.SendAlert("Oops", "Something went wrong. Please try again later.");
				return;
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}
