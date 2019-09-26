using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Clients.Portable;
using System.Windows.Input;
using ImageCircle.Forms.Plugin.Abstractions;
using System.Linq;

namespace XamarinEvolve.Clients.UI
{

    public class NonSessionCell : ViewCell
    {
        readonly INavigation navigation;

        public NonSessionCell(INavigation navigation = null)
        {
            View = new NonSessionCellView();
            this.navigation = navigation;
        }
    }

    public partial class NonSessionCellView : ContentView
    {
        public NonSessionCellView()
        {
            InitializeComponent();
        }
    }
}

