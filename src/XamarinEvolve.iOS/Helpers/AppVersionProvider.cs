using Foundation;
using UIKit;
using Xamarin.Forms;
using XamarinEvolve.iOS;
using XamarinEvolve.Utils;

[assembly: Dependency(typeof(AppVersionProvider))]

namespace XamarinEvolve.iOS
{
	public class AppVersionProvider: IAppVersionProvider
	{
		public string AppVersion => NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleShortVersionString")].ToString();

		public bool SupportsWebRtc => UIDevice.CurrentDevice.CheckSystemVersion(11,0);
	}
}

