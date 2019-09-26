using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AppCenter.Analytics;
using Plugin.GoogleAnalytics;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Utils;

[assembly: Dependency(typeof(EvolveLogger))]
namespace XamarinEvolve.Clients.Portable
{
	public class EvolveLogger : ILogger
    {
		private static bool googleAnalyticsEnabled = false; // start with disabled google tracking untill properly intialized. don't block startup of the app by tracking

        static EvolveLogger()
        {
            if (FeatureFlags.GoogleAnalyticsEnabled)
            {
                SetupGoogleAnalytics();
            }
        }

        private static void SetupGoogleAnalytics()
        {
            try
            {
				GoogleAnalytics.Current.Config.TrackingId = ApiKeys.GoogleAnalyticsTrackingId;
				GoogleAnalytics.Current.Config.AppId = AboutThisApp.PackageName;
				GoogleAnalytics.Current.Config.AppName = AboutThisApp.AppName;
				GoogleAnalytics.Current.Config.AppInstallerId = Settings.Current.UserIdentifier;
				GoogleAnalytics.Current.Config.UseSecure = true;
#if DEBUG
                GoogleAnalytics.Current.Config.Debug = true;
#endif

				GoogleAnalytics.Current.InitTracker();
				googleAnalyticsEnabled = true;
			}
            catch (Exception ex)
            {
				Dump($"Google Analytics initialization threw exception:", ex);
                googleAnalyticsEnabled = false;
            }
        }

        #region ILogger implementation

        public virtual void TrackPage(string page, string id = null)
        {
			try
			{
				Dump($"Tracking page: {page} Id: {id}");

				if (FeatureFlags.HockeyAppEnabled)
				{
					Analytics.TrackEvent($"{page}Page:{id ?? string.Empty}");
				}

				if (googleAnalyticsEnabled)
				{
					GoogleAnalytics.Current.Tracker.SendView($"{page}Page:{id ?? string.Empty}");
				}
			}
			catch (Exception ex)
			{
				Dump($"TrackPage threw exception:", ex);
			}
        }

        public virtual void TrackTimeSpent(string page, string id, TimeSpan time)
        {
			try
			{
				Dump($"Tracking time spent: {page} Id: {id} Time: {time.TotalMilliseconds} ms");

				if (googleAnalyticsEnabled)
				{
					GoogleAnalytics.Current.Tracker.SendTiming(time, "TimeSpentOnPage", page, id);
				}
			}
            catch (Exception ex)
            {
				Dump($"TrackTimeSpent threw exception:", ex);
			}
		}

		public virtual void Track(string trackIdentifier)
        {
			try
			{
				Dump($"Tracking: {trackIdentifier}");

				if (FeatureFlags.HockeyAppEnabled)
				{
					Analytics.TrackEvent(trackIdentifier);
				}

				if (googleAnalyticsEnabled)
				{
					GoogleAnalytics.Current.Tracker.SendEvent("General", trackIdentifier);
				}
			}
			catch (Exception ex)
			{
				Dump($"TrackPage threw exception:", ex);
			}
		}

		public virtual void Track(string trackIdentifier, string key, string value)
        {
			try
			{
				Dump("Tracking: " + trackIdentifier + " key: " + key + " value: " + value);

				var fullTrackIdentifier = $"{trackIdentifier}-{key}-{@value}";

				if (FeatureFlags.HockeyAppEnabled)
				{
					Analytics.TrackEvent(fullTrackIdentifier);
				}

				if (googleAnalyticsEnabled)
				{
					GoogleAnalytics.Current.Tracker.SendEvent("General", fullTrackIdentifier, value);
				}
			}
			catch (Exception ex)
			{
				Dump($"Track threw exception:", ex);
			}
		}

		public virtual void Report(Exception exception = null, Severity warningLevel = Severity.Warning)
        {
			try
			{
				Dump("Reporting: ", exception);

				if (googleAnalyticsEnabled)
				{
					GoogleAnalytics.Current.Tracker.SendException(exception, warningLevel == Severity.Critical);
				}
			}
			catch (Exception ex)
			{
				Dump($"Report threw exception:", ex);
			}
		}

		public virtual void Report(Exception exception, IDictionary<string, string> extraData, Severity warningLevel = Severity.Warning)
        {
			try
			{
				Dump("Reporting: ", exception);

				if (googleAnalyticsEnabled)
				{
					GoogleAnalytics.Current.Tracker.SendException(exception, warningLevel == Severity.Critical);
				}
			}
			catch (Exception ex)
			{
				Dump($"Report threw exception:", ex);
			}
		}

		public virtual void Report(Exception exception, string key, string value, Severity warningLevel = Severity.Warning)
        {
			try
			{
				Dump($"Reporting: (key:{key},value:{value})", exception);

				if (googleAnalyticsEnabled)
				{
					GoogleAnalytics.Current.Tracker.SendException(exception, warningLevel == Severity.Critical);
				}
			}
			catch (Exception ex)
			{
				Dump($"Report threw exception:", ex);
			}
		}

		private static void Dump(string message, Exception exception = null)
		{
#if __IOS__
			Console.WriteLine($"~~ Logger: {message} {exception?.Flatten()}");
#else
			Debug.WriteLine($"~~ Logger: {message} {exception?.Flatten()}");
#endif
		}
		#endregion
	}
}
