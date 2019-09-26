using System;
using System.Collections.Generic;
using System.Linq;
using MvvmHelpers;
using XamarinEvolve.DataObjects;
using Xamarin.Forms;
using XamarinEvolve.Utils;
using System.Diagnostics;

namespace XamarinEvolve.Clients.Portable
{
	public static class SessionExtensions
	{
        public static AppLinkEntry GetAppLink(this Session session)
		{
			var url = $"http://{AboutThisApp.AppLinksBaseDomain}/{AboutThisApp.SessionsSiteSubdirectory.ToLowerInvariant()}/{session.Id}";

			var entry = new AppLinkEntry
			{
				Title = session.Title ?? "",
				Description = session.Abstract ?? "",
				AppLinkUri = new Uri(url, UriKind.RelativeOrAbsolute),
				IsLinkActive = true
			};

			if (Device.RuntimePlatform == Device.iOS)
				entry.Thumbnail = ImageSource.FromFile("Icon.png");

			entry.KeyValues.Add("contentType", "Session");
			entry.KeyValues.Add("appName", AboutThisApp.AppName);
			entry.KeyValues.Add("companyName", AboutThisApp.CompanyName);

			return entry;
		}

		public static string GetWebUrl(this Session session)
		{
			return $"http://{AboutThisApp.AppLinksBaseDomain}/{AboutThisApp.SessionsSiteSubdirectory}/#{session.Id}";
		}

		public static string GetIndexName(this Session e)
		{
			if (!e.StartTime.HasValue || !e.EndTime.HasValue || e.StartTime.Value.IsTBA())
				return "To be announced";

			var start = e.StartTime.Value.ToEventTimeZone();

			var startString = start.ToString("t", EventInfo.Culture);
			var end = e.EndTime.Value.ToEventTimeZone();
			var endString = end.ToString("t", EventInfo.Culture);

			var day = start.DayOfWeek.ToString();
			var monthDay = start.ToString("M");
			return $"{day}, {monthDay}, {startString}–{endString}";
		}

		public static string GetGroupName(this Session session)
		{
			if (!session.StartTime.HasValue || !session.EndTime.HasValue || session.StartTime.Value.IsTBA())
				return "To be announced";

			var start = session.StartTime.Value.ToEventTimeZone();
			var startString = start.ToString("t", EventInfo.Culture);

			if (Clock.Now.Year == start.Year)
			{
				if (Clock.Now.DayOfYear == start.DayOfYear)
					return $"Today {startString}";

				if (Clock.Now.DayOfYear + 1 == start.DayOfYear)
					return $"Tomorrow {startString}";
			}
			var day = start.ToString("M");
			return $"{day}, {startString}";
		}

		public static string GetDisplayName(this Session session)
		{
			if (!session.StartTime.HasValue || !session.EndTime.HasValue || session.StartTime.Value.IsTBA())
				return "TBA";

			var start = session.StartTime.Value.ToEventTimeZone();
			var startString = start.ToString("t", EventInfo.Culture);
			var end = session.EndTime.Value.ToEventTimeZone();
			var endString = end.ToString("t", EventInfo.Culture);

			var location = string.Empty;
			if (FeatureFlags.ShowLocationInSessionCell)
			{
				if (session.Room != null)
				{
					location = $", {session.Room.Name}";
				}
			}

			if (Clock.Now.Year == start.Year)
			{
				if (Clock.Now.DayOfYear == start.DayOfYear)
					return $"Today {startString}–{endString}{location}";

				if (Clock.Now.DayOfYear + 1 == start.DayOfYear)
					return $"Tomorrow {startString}–{endString}{location}";
			}

			var day = start.ToString("M");
            return $"{day}, {startString}–{endString}{location}";
		}


		public static string GetDisplayTime(this Session session)
		{
			if (!session.StartTime.HasValue || !session.EndTime.HasValue || session.StartTime.Value.IsTBA())
				return "TBA";
			var start = session.StartTime.Value.ToEventTimeZone();

			var startString = start.ToString("t", EventInfo.Culture);
			var end = session.EndTime.Value.ToEventTimeZone();
            var endString = end.ToString("t", EventInfo.Culture);
			var location = string.Empty;
			if (FeatureFlags.ShowLocationInSessionCell)
			{
				if (session.Room != null)
				{
					location = $", {session.Room.Name}";
				}
			} 
			return $"{startString}–{endString}{location}";
		}

        public static IEnumerable<Grouping<string, Session>> FilterAndGroupByDate(this IList<Session> sessions, DateTime referenceDate, bool favoritesOnly, bool includePastSessions, bool includeAllCategories, string filteredCategories)
        {
            if (favoritesOnly)
			{
				sessions = sessions.Where(s => s.IsFavorite).ToList();
			}

			var filteredCategoriesList = filteredCategories.Split('|');

			//is not tba
			//has not started or has started and hasn't ended or ended 20 minutes ago
			//filter then by category and filters
			var grouped = (from session in sessions
                           where session.StartTime.HasValue && session.EndTime.HasValue 
			               && !session.StartTime.Value.IsTBA() && (includePastSessions || referenceDate <= session.EndTime.Value.ToUniversalTime().AddMinutes(AppBehavior.ShowPastSessionsTimeWindowInMinutes))
                           && (includeAllCategories || (session?.Categories.Join(filteredCategoriesList, category => category.Name, filtered => filtered, (category, filter) => filter).Any() ?? false))
						   orderby session.StartTimeOrderBy, session.Title
                           group session by session.GetGroupName()
						   into sessionGroup
						   select new Grouping<string, Session>(sessionGroup.Key, sessionGroup)).ToList();

			var tba = sessions.Where(s => !s.StartTime.HasValue || !s.EndTime.HasValue || s.StartTime.Value.IsTBA());
			if (tba.Any())
			{
				var tbaFiltered = (from session in tba
                                   where (includeAllCategories || (session?.Categories.Join(filteredCategoriesList, category => category.Name, filtered => filtered, (category, filter) => filter).Any() ?? false))
								   select session).ToList();

				grouped.Add(new Grouping<string, Session>("TBA", tbaFiltered));
			}

			return grouped;            
        }

		public static IEnumerable<Grouping<string, Session>> FilterAndGroupByDate(this IList<Session> sessions)
		{
            return FilterAndGroupByDate(sessions, Clock.Now, Settings.Current.FavoritesOnly, Settings.Current.ShowPastSessions, Settings.Current.ShowAllCategories, Settings.Current.FilteredCategories);
		}

		public static IEnumerable<Session> Search(this IEnumerable<Session> sessions, string searchText)
		{
			if (string.IsNullOrWhiteSpace(searchText))
				return sessions;

			var searchSplit = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

			//search title, then category, then speaker name
			return sessions.Where(session =>
								  searchSplit.Any(search =>
								session.Haystack.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));
		}
	}
}

