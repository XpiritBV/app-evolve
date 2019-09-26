using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Clients.Portable;
using System.Windows.Input;
using ImageCircle.Forms.Plugin.Abstractions;
using System.Linq;

namespace XamarinEvolve.Clients.UI
{

    public class SessionCellOnSpeakerPage: ViewCell
    {
        readonly INavigation navigation;

        public SessionCellOnSpeakerPage (INavigation navigation = null)
        {
            View = new SessionCellForSpeakerDetailView ();
            this.navigation = navigation;
        }

        protected override async void OnTapped()
        {
            base.OnTapped();
            if (navigation == null)
                return;
            var session = BindingContext as Session;
            if (session == null)
                return;
            
            App.Logger.TrackPage(AppPage.Session.ToString(), session.Title);
            await NavigationService.PushAsync(navigation, new SessionDetailsPage(session));
        }
    }

	public partial class SessionCellForSpeakerDetailView : ContentView
	{
		public SessionCellForSpeakerDetailView()
		{
			InitializeComponent();
		}

		public static readonly BindableProperty FavoriteCommandProperty =
			BindableProperty.Create(nameof(FavoriteCommand), typeof(ICommand), typeof(SessionCellView), default(ICommand));

		public ICommand FavoriteCommand
		{
			get { return GetValue(FavoriteCommandProperty) as Command; }
			set { SetValue(FavoriteCommandProperty, value); }
		}
	}
}

