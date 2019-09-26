using System.Threading.Tasks;

using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.UI
{
    public partial class ConferenceFeedbackPage : BasePage
    {
		public override AppPage PageType => AppPage.ConferenceFeedback;
		ConferenceFeedbackViewModel vm;

        public ConferenceFeedbackPage()
        {
            InitializeComponent();

			BindingContext = vm = new ConferenceFeedbackViewModel(Navigation);

			if (Device.RuntimePlatform != Device.iOS)
				ToolbarDone.IconImageSource = "toolbar_close.png";

            ToolbarDone.Command = new Command(() => Done().IgnoreResult(vm.ShowError), () => !vm.IsBusy);
        }

        private async Task Done()
        {
            if (vm.IsBusy)
            {
                return;
            }

            await Navigation.PopModalAsync(true);
        }

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			Question1.RemoveBehaviors();
			Question2.RemoveBehaviors();
			Question3.RemoveBehaviors();
			Question4.RemoveBehaviors();
			Question5.RemoveBehaviors();
			Question6.RemoveBehaviors();
			Question7.RemoveBehaviors();
		}
    }
}
