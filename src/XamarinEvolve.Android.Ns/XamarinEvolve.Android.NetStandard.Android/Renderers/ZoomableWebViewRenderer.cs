using Xamarin.Forms;
using XamarinEvolve.Clients.UI;
using XamarinEvolve.Droid;

[assembly: ExportRenderer(typeof(ZoomableWebView), typeof(ZoomableWebViewRenderer))]

namespace XamarinEvolve.Droid
{
    public class ZoomableWebViewRenderer: Xamarin.Forms.Platform.Android.WebViewRenderer
    {
		public ZoomableWebViewRenderer() : base(MainApplication.ActivityContext)
		{ }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
			base.OnElementChanged(e);

			if (Control != null)
			{
                Control.SetWebChromeClient(new global::Android.Webkit.WebChromeClient());
                Control.Settings.JavaScriptEnabled = true;
				Control.Settings.BuiltInZoomControls = true;
				Control.Settings.DisplayZoomControls = true;
                Control.Settings.AllowFileAccessFromFileURLs = true;
                Control.Settings.AllowUniversalAccessFromFileURLs = true;
                Control.Settings.AllowFileAccess = true;
                Control.Settings.AllowContentAccess = true;

				Control.SetInitialScale(1);
				Control.Settings.LoadWithOverviewMode = true;
				Control.Settings.UseWideViewPort = true;
			}
        }
    }
}
