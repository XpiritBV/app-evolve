using System.Linq;
using System.Threading.Tasks;
using CoreSpotlight;
using Foundation;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.DataObjects;
using XamarinEvolve.iOS.PlatformFeatures.ProActiveSuggestions;
using XamarinEvolve.Utils;

[assembly: Dependency(typeof(SpeakerUserActivity))]

namespace XamarinEvolve.iOS.PlatformFeatures.ProActiveSuggestions
{
	public class SpeakerUserActivity : IPlatformSpecificExtension<Speaker>
	{
        private readonly IDependencyService _locator;
		private NSUserActivity _activity;

        public SpeakerUserActivity() : this(new DependencyServiceWrapper())
        { }

		public SpeakerUserActivity(IDependencyService locator)
        {
            _locator = locator;
        }

		public Task Execute(Speaker entity)
		{
			if (_activity != null)
			{
				_activity.Invalidate();
			}

			_activity = new NSUserActivity($"{AboutThisApp.PackageName}.speaker")
			{
				Title = entity.FullName
			};

			RegisterHandoff(entity);

			_activity.BecomeCurrent();

			return Task.CompletedTask;
		}

		public Task Finish()
		{
			_activity?.ResignCurrent();
			return Task.CompletedTask;
		}

		void RegisterHandoff(Speaker speaker)
		{
			var userInfo = new NSMutableDictionary();
            var appLink =  speaker.GetAppLink(_locator.Get<IImageChecker>());

            userInfo.Add(new NSString("Url"), new NSString(appLink.AppLinkUri.AbsoluteUri));

			var keywords = new NSMutableSet<NSString>(new NSString(speaker.FirstName), new NSString(speaker.LastName));
			if (speaker.Sessions != null)
			{
				foreach (var session in speaker.Sessions)
				{
					keywords.Add(new NSString(session.Title));
				}
			}

			_activity.Keywords = new NSSet<NSString>(keywords);
			_activity.WebPageUrl = NSUrl.FromString(speaker.GetWebUrl());

			_activity.EligibleForHandoff = false;

			_activity.AddUserInfoEntries(userInfo);

			// Provide context
			var attributes = new CSSearchableItemAttributeSet($"{AboutThisApp.PackageName}.speaker");
			attributes.Keywords = keywords.ToArray().Select(k => k.ToString()).ToArray();
			attributes.Url = NSUrl.FromString(appLink.AppLinkUri.AbsoluteUri);
			_activity.ContentAttributeSet = attributes;
		}
	}
}
