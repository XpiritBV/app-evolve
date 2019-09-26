
using Xamarin.Forms;

namespace XamarinEvolve.Clients.UI
{
    public class EvolveNavigationPage : NavigationPage
    {
        public EvolveNavigationPage(Page root) : base(root)
        {
            Init();
            Title = root.Title;
            IconImageSource = root.IconImageSource;
			
		}

        public EvolveNavigationPage()
        {
            Init();
        }

        void Init()
        {
			BarBackgroundColor = (Color)App.Current.Resources["BarBackgroundColor"];
			BarTextColor = (Color)App.Current.Resources["Accent"];

		}
	}
}

