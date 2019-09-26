using System.Net.Http;
using Xamarin.Android.Net;
using Xamarin.Forms;
using XamarinEvolve.Droid;
using XamarinEvolve.Utils;

[assembly: Dependency(typeof(DroidMessageHandlerProvider))]

namespace XamarinEvolve.Droid
{
    public class DroidMessageHandlerProvider : IMessageHandlerProvider
    {
        public HttpMessageHandler GetHandler()
        {
            return new AndroidClientHandler();
        }
    }
}
