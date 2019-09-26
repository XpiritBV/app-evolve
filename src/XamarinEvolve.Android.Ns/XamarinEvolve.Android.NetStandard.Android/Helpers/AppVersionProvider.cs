using Xamarin.Forms;
using XamarinEvolve.Droid;
using XamarinEvolve.Utils;

[assembly: Dependency(typeof(AppVersionProvider))]

namespace XamarinEvolve.Droid
{
	public class AppVersionProvider : IAppVersionProvider
	{
		public string AppVersion
		{
			get
			{
				var context = global::Android.App.Application.Context;
				return context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
			}
		}

		public bool SupportsWebRtc => global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Lollipop;
	}
}

