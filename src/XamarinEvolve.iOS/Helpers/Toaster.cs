using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.iOS;
using GlobalToast;
using GlobalToast.Animation;
using UIKit;

[assembly: Dependency(typeof(Toaster))]
namespace XamarinEvolve.iOS
{
    public class Toaster : IToast
    {
        public void SendToast(string message)
        {
	
			Device.BeginInvokeOnMainThread(() =>
                {
					Toast.GlobalAnimator = new ScaleAnimator();
					Toast.GlobalLayout.MarginBottom = 16f;
					Toast.GlobalAppearance.MessageColor = UIColor.Red;
					Toast.GlobalAppearance.TitleFont = UIFont.SystemFontOfSize(16, UIFontWeight.Light);

					// Or you can replace entire objects
					Toast.GlobalAppearance = new ToastAppearance
					{
						Color = UIColor.Blue,
						CornerRadius = 5
					};

					Toast.MakeToast(message)
						 .Show();
				});
        }
    }
}
