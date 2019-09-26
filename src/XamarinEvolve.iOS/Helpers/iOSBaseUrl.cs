using Foundation;
using Xamarin.Forms;
using XamarinEvolve.iOS;
using XamarinEvolve.Utils;

[assembly: Dependency(typeof(iOSBaseUrl))]

namespace XamarinEvolve.iOS
{
    public class iOSBaseUrl : IBaseUrl
    {
        public string Get()
        {
            return NSBundle.MainBundle.BundlePath;
        }
    }
}
