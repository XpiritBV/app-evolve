using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Clients.Portable;

namespace XamarinEvolve.Clients.UI
{
    public class SpeakerCell: ViewCell
    {
        readonly INavigation navigation;
        string sessionId;
        public SpeakerCell (string sessionId, INavigation navigation = null)
        {
            this.sessionId = sessionId;
            View = new SpeakerCellView ();
            StyleId = "disclosure";
            this.navigation = navigation;
        }
    }

    public partial class SpeakerCellView : ContentView
    {
        public SpeakerCellView()
        {
            InitializeComponent();
        }
    }
}

