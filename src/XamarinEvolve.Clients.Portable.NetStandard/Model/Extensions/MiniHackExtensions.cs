using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
	public static class MiniHackExtensions
	{
        public static AppLinkEntry GetAppLink(this MiniHack miniHack, IImageChecker imageChecker)
		{
			var url = $"http://{AboutThisApp.AppLinksBaseDomain}/{AboutThisApp.MiniHacksSiteSubdirectory.ToLowerInvariant()}/{miniHack.Id}";

			var entry = new AppLinkEntry
			{
				Title = miniHack.Name ?? "",
				Description = miniHack.Description ?? "",
				AppLinkUri = new Uri(url, UriKind.RelativeOrAbsolute),
				IsLinkActive = true
			};

			if (Device.RuntimePlatform == Device.iOS)
			{
				if (imageChecker.IsImageUsableForAppLink(miniHack.BadgeUrl).Result)
				{
					entry.Thumbnail = ImageSource.FromUri(miniHack.BadgeUri);
				}
				else
				{
					entry.Thumbnail = ImageSource.FromFile("Icon.png");
				}
			}

			entry.KeyValues.Add("contentType", "Session");
			entry.KeyValues.Add("appName", AboutThisApp.AppName);
			entry.KeyValues.Add("companyName", AboutThisApp.CompanyName);

			return entry;
		}
	}
}
