using System.Net.Http;
using Xamarin.Forms;
using XamarinEvolve.iOS;
using XamarinEvolve.Utils;

[assembly: Dependency(typeof(iOSMessageHandlerProvider))]

namespace XamarinEvolve.iOS
{
    public class iOSMessageHandlerProvider : IMessageHandlerProvider
    {
        public HttpMessageHandler GetHandler()
        {
            return new NSUrlSessionHandler();
        }
    }
}
