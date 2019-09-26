using System;
using Xamarin.Forms;
using System.Windows.Input;
using System.Threading.Tasks;
using Plugin.Share;
using FormsToolkit;
using Plugin.Share.Abstractions;
using XamarinEvolve.Utils;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.Clients.Portable
{
    public class LoginViewModel : ViewModelBase
    {
        ISSOClient client;
        public LoginViewModel(INavigation navigation) : base(navigation)
        {
            client = Locator.Get<ISSOClient>();
        }

        protected override void UpdateCommandCanExecute()
        {
            loginCommand?.ChangeCanExecute();
            cancelCommand?.ChangeCanExecute();
            signupCommand?.ChangeCanExecute();
        }

        string message;
        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }

        string email;
        public string Email
        {
            get { return email; }
            set { SetProperty(ref email, value); }
        }
        string password;
        public string Password
        {
            get { return password; }
            set { SetProperty(ref password, value); }
        }

        Command  loginCommand;
        public ICommand LoginCommand =>
            loginCommand ?? (loginCommand = new Command(async () => await ExecuteLoginAsync())); 

        async Task ExecuteLoginAsync()
        {
			if (IsBusy)
			{
				return;
			}

            if(string.IsNullOrWhiteSpace(email))
            {
                MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
                    {
                        Title="Sign in Information",
                        Message="We do need your email address :-)",
                        Cancel ="OK"
                    });
                return;
            }

            if(string.IsNullOrWhiteSpace(password))
            {
                MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
                    {
                        Title="Sign in Information",
                        Message="Password is empty!",
                        Cancel ="OK"
                    });
                return;
            }

            try 
            {
                IsBusy = true;
                Message = "Signing in...";
                #if DEBUG
                await Task.Delay(1000);
                #endif
                AccountResponse result = null;

                if(result == null)
                    result = await client.LoginAsync(email, password);
                
                if(result?.Success ?? false)
                {
                    Message = "Updating schedule...";
                    Settings.FirstName = result.User?.FirstName ?? string.Empty;
                    Settings.LastName = result.User?.LastName ?? string.Empty;
                    Settings.Email = email.ToLowerInvariant();
					Settings.UserIdentifier = email.ToLowerInvariant();
                    MessagingService.Current.SendMessage(MessageKeys.LoggedIn);
                    Logger.Track(EvolveLoggerKeys.LoginSuccess);
                    try
                    {
                        await StoreManager.SyncAllAsync(true);
                        Settings.Current.LastSync = Clock.Now;
                        Settings.Current.HasSyncedData = true;
                    }
                    catch(Exception ex)
                    {
                        //if sync doesn't work don't worry it is alright we can recover later
                        Logger.Report(ex);
                    }
                    await Finish();
                    Settings.FirstRun = false;
                }
                else
                {
                    Logger.Track(EvolveLoggerKeys.LoginFailure, "Reason", result.Error); 
                    MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
                        {
                            Title="Unable to Sign in",
                            Message=result.Error,
                            Cancel ="OK"
                        });
                }
            } 
            catch (Exception ex) 
            {
                Logger.Track(EvolveLoggerKeys.LoginFailure, "Reason", ex?.Message ?? string.Empty);

                MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message, new MessagingServiceAlert
                    {
                        Title="Unable to Sign in",
                        Message="The email or password provided is incorrect.",
                        Cancel ="OK"
                    });
            }
            finally
            {
                Message = string.Empty;
                IsBusy = false;
            }
        }

        Command  signupCommand;
        public ICommand SignupCommand =>
            signupCommand ?? (signupCommand = new Command(() => ExecuteSignupAsync().IgnoreResult(ShowError))); 

        async Task ExecuteSignupAsync()
        {
            Logger.Track(EvolveLoggerKeys.Signup);

			var primaryColor = ((Color)Application.Current.Resources["Primary"]);
			var tintColor = new ShareColor
			{
				A = 255,
				R = Convert.ToInt32(primaryColor.R * 256),
				G = Convert.ToInt32(primaryColor.G * 256),
				B = Convert.ToInt32(primaryColor.B * 256)
			};

            await CrossShare.Current.OpenBrowser("https://auth.xamarin.com/account/register",
                new BrowserOptions
                {
                    ChromeShowTitle = true,
                    ChromeToolbarColor = tintColor,
                    SafariControlTintColor = tintColor,
                    UseSafariReaderMode = false,
                    UseSafariWebViewController = true
                });
        }

        Command  cancelCommand;
        public ICommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new Command(() => ExecuteCancelAsync().IgnoreResult(ShowError))); 

        async Task ExecuteCancelAsync()
        {
            Logger.Track(EvolveLoggerKeys.LoginCancel);
            if(Settings.FirstRun)
            {
                try 
                {
                    Message = "Updating schedule...";
                    IsBusy = true;
                    await StoreManager.SyncAllAsync(false);
                    Settings.Current.LastSync = Clock.Now;
                    Settings.Current.HasSyncedData = true;
                } 
                catch (Exception ex) 
                {
                    //if sync doesn't work don't worry it is alright we can recover later
                    Logger.Report(ex);
                }
                finally
                {
                    Message = string.Empty;
                    IsBusy = false;
                }
            }
            await Finish();
            Settings.FirstRun = false;
        }

        async Task Finish()
        {
            if(Device.RuntimePlatform == Device.iOS && Settings.FirstRun)
            {

                    var push = Locator.Get<IPushNotifications>();
                    if(push != null)
                        await push.RegisterForNotifications();

                    await Navigation.PopModalAsync();

			}
            else
            {
                await Navigation.PopModalAsync();
            }
        }
    }
}

