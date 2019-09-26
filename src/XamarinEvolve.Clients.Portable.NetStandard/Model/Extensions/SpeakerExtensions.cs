using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
	public static class SpeakerExtensions
	{
        public static AppLinkEntry GetAppLink(this Speaker speaker, IImageChecker imageChecker)
		{
			var url = $"http://{AboutThisApp.AppLinksBaseDomain}/{AboutThisApp.SpeakersSiteSubdirectory.ToLowerInvariant()}/{speaker.Id}";

			var entry = new AppLinkEntry
			{
				Title = speaker.FullName ?? "",
				Description = speaker.Biography ?? "",
				AppLinkUri = new Uri(url, UriKind.RelativeOrAbsolute),
				IsLinkActive = true,
			};

			if (Device.RuntimePlatform == Device.iOS)
			{
				if ( imageChecker.IsImageUsableForAppLink(speaker.AvatarUrl).Result)
				{
					entry.Thumbnail = ImageSource.FromUri(speaker.AvatarUri);
				}
				if (entry.Thumbnail == null)
				{
					entry.Thumbnail = ImageSource.FromFile("Icon.png");
				}
			}

			entry.KeyValues.Add("contentType", "Speaker");
			entry.KeyValues.Add("appName", AboutThisApp.AppName);
			entry.KeyValues.Add("companyName", AboutThisApp.CompanyName);

			return entry;
		}

		public static string GetWebUrl(this Speaker speaker)
		{
			return $"http://{AboutThisApp.AppLinksBaseDomain}/{AboutThisApp.SpeakersSiteSubdirectory}/#{speaker.Id}";
		}
	}
}

