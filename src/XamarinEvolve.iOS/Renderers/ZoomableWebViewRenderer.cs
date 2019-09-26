using UIKit;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinEvolve.Clients.UI;
using XamarinEvolve.iOS;

[assembly: ExportRenderer(typeof(ZoomableWebView), typeof(ZoomableWebViewRenderer))]

namespace XamarinEvolve.iOS
{
	public class ZoomableWebViewRenderer : WebViewRenderer
	{
		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			if (NativeView is UIWebView view)
			{
				view.ScalesPageToFit = true;
			}
		}
	}
}
