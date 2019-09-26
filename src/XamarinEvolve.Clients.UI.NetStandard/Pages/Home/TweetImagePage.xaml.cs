using System;
using Xamarin.Forms;
using System.Diagnostics;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.UI
{
	public partial class TweetImagePage : BasePage
	{
		public override AppPage PageType => AppPage.TweetImage;

        public TweetImagePage(string image)
        {
            InitializeComponent();
            var item = new ToolbarItem
            {
                Text = "Done",
                Command = new Command(() => Navigation.PopModalAsync().IgnoreResult())
            };
            if (Device.RuntimePlatform == Device.Android)
                item.IconImageSource = "toolbar_close.png";
            ToolbarItems.Add(item);

            try
            {
                MainImage.Source = new UriImageSource
                {
                    Uri = new Uri(image),
                    CachingEnabled = true,
                    CacheValidity = TimeSpan.FromDays(3)
                };
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Unable to convert image to URI: " + ex);
                DependencyService.Get<IToast>().SendToast("Unable to load image.");
            }

            MainImage.PropertyChanged += (sender, e) => 
                {
                    if(e.PropertyName != nameof(MainImage.IsLoading))
                        return;
                    ProgressBar.IsRunning = MainImage.IsLoading;
                    ProgressBar.IsVisible = MainImage.IsLoading;
                };
        }
	}
}

