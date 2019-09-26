using Xamarin.Forms;
using XamarinEvolve.Droid;
using XamarinEvolve.Utils;

[assembly: Dependency(typeof(DroidBaseUrl))]

namespace XamarinEvolve.Droid
{
    public class DroidBaseUrl : IBaseUrl
    {
        public string Get()
        {
            return "file:///android_asset/";
        }
    }
}
