using System;
using Foundation;
using UIKit;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.iOS;

[assembly: Dependency(typeof(LaunchLinkedIn))]

namespace XamarinEvolve.iOS
{
    public class LaunchLinkedIn : ILaunchLinkedIn
    {
        public bool OpenProfile(string profile, string type)
        {
			try
			{
                var linkedInUrl = NSUrl.FromString($"linkedin://{type}/{profile}");
				if (UIApplication.SharedApplication.OpenUrl(linkedInUrl))
					return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Unable to launch url " + ex);
			}
            return false;

        }
    }
}
