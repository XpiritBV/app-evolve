using Xamarin.Forms;
using XamarinEvolve.DataStore.Abstractions;
using MvvmHelpers;
using Plugin.Share;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Share.Abstractions;
using System;
using XamarinEvolve.Utils;
using FormsToolkit;
using System.Diagnostics;

namespace XamarinEvolve.Clients.Portable
{
	public class ViewModelBase : BaseViewModel, IDisposable
    {
        protected INavigation Navigation { get; }
        protected IDependencyService Locator { get; }
		public DateTime NextForceRefresh { get; set; }

		public ViewModelBase(INavigation navigation = null) : this (new DependencyServiceWrapper())
        {
            Navigation = navigation;
            PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(IsBusy))
                {
					Device.BeginInvokeOnMainThread(() =>
					{
						launchBrowserCommand?.ChangeCanExecute();
						UpdateCommandCanExecute();
					});
                }
            };
        }

        public virtual void Activate()
        {
            if (NextForceRefresh == DateTime.MinValue)
            {
                NextForceRefresh = Settings.LastSync.AddMinutes(AppBehavior.RefreshIntervalInMinutes);
            }
		}

        public virtual void Deactivate()
        {
            
        }

        protected virtual void UpdateCommandCanExecute()
        {
            
        }

        public ViewModelBase(IDependencyService locator)
        {
			Locator = locator;
			Logger = locator.Get<ILogger>();
            StoreManager = locator.Get<IStoreManager>();
			Toast = locator.Get<IToast>();
            FavoriteService = locator.Get<FavoriteService>();
		}

		public void ShowError(Exception ex)
		{
			Logger.Report(ex);
			MessagingService.Current.SendMessage(MessageKeys.Error, ex);
		}

        Stopwatch _timer = new Stopwatch();

        [Conditional("DEBUG")]
        protected void StartTimer()
        {
            _timer.Reset();
            _timer.Start();
        }

        [Conditional("DEBUG")]
        protected void DumpTiming(string message)
		{
			_timer.Stop();
			Debug.WriteLine($"{message} - {_timer.ElapsedMilliseconds}ms");
			_timer.Restart();
		}

        protected ILogger Logger { get; }
        protected IStoreManager StoreManager { get; }
        protected IToast Toast { get; }
        protected FavoriteService FavoriteService { get; }

        public Settings Settings
        {
            get { return Settings.Current; }
        }

        Command  launchBrowserCommand;
        public ICommand LaunchBrowserCommand =>
            launchBrowserCommand ?? (launchBrowserCommand = new Command<string>((t) => ExecuteLaunchBrowserAsync(t).IgnoreResult(ShowError),(arg) => !IsBusy)); 

        async Task ExecuteLaunchBrowserAsync(string arg)
        {
            if(IsBusy || string.IsNullOrEmpty(arg))
                return;

            if (!arg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !arg.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                arg = "http://" + arg;
            }
			arg = arg.Trim();

			var lower = arg.ToLowerInvariant();

			Logger.Track(EvolveLoggerKeys.LaunchedBrowser, "Url", lower);

			if (Device.RuntimePlatform == Device.iOS)
			{
				if (lower.Contains("twitter.com"))
				{
					try
					{
						var id = arg.Substring(lower.LastIndexOf("/", StringComparison.Ordinal) + 1);
                        var launchTwitter = Locator.Get<ILaunchTwitter>();
						if (lower.Contains("/status/"))
						{
                            //status
                            if (launchTwitter.OpenStatus(id))
                            {
                                return;
                            }
						}
						else
						{
                            //user
                            if (launchTwitter.OpenUserName(id))
                            {
                                return;
                            }
						}
					}
					catch (Exception e)
					{
                        Logger.Report(e, Severity.Warning);
					}
				}
				if (lower.Contains("facebook.com"))
				{
					try
					{
						var id = arg.Substring(lower.LastIndexOf("/", StringComparison.Ordinal) + 1);
                        var launchFacebook = Locator.Get<ILaunchFacebook>();
                        if (launchFacebook.OpenUserName(id))
                        {
                            return;
                        }
					}
                    catch (Exception e)
					{
                        Logger.Report(e, Severity.Warning);
					}
				}
				if (lower.Contains("linkedin.com"))
				{
					try
					{
                        var parts = lower.Split('/');
                        string type = "profile";

                        if (lower.Contains("company"))
                        {
                            type = "company";
                        }

						var id = arg.Substring(lower.LastIndexOf("/", StringComparison.Ordinal) + 1);
                        var launchLinkedIn = Locator.Get<ILaunchLinkedIn>();
                        if (launchLinkedIn.OpenProfile(id, type))
						{
							return;
						}
					}
					catch (Exception e)
					{
						Logger.Report(e, Severity.Warning);
					}
				}

			}

            try 
            {
				var primaryColor = ((Color)Application.Current.Resources["Primary"]);
				var tintColor = new ShareColor
				{
					A = 255,
					R = Convert.ToInt32(primaryColor.R * 256),
					G = Convert.ToInt32(primaryColor.G * 256),
					B = Convert.ToInt32(primaryColor.B * 256)
				};

				await CrossShare.Current.OpenBrowser (arg, new BrowserOptions {
                    ChromeShowTitle = true,
                    ChromeToolbarColor = tintColor,
                    SafariControlTintColor = tintColor,
                    UseSafariReaderMode = false,
                    UseSafariWebViewController = true
                });
            } 
            catch (Exception e)
            {
                Logger.Report(e, Severity.Warning);
            }
        }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// clean up subscriptions etc
			}
		}
	}
}


