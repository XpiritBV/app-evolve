// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System.Threading.Tasks;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Mock
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings _current = CrossSettings.Current;
		private static IDependencyService Locator = new DependencyServiceWrapper();

		public static void SetProviders (ISettings provider, IDependencyService locator)
        {
            _current = provider;
            Locator = locator;
        }

        static ISettings AppSettings
        {
            get
            {
                return _current;
            }
        }

        public static bool IsFavorite(string id) =>
            AppSettings.GetValueOrDefault("fav_"+id, false);

        public static void SetFavorite(string id, bool favorite) =>
            AppSettings.AddOrUpdateValue("fav_"+id, favorite);

        public static async Task ClearFavorites()
        {
            var sessions = await Locator.Get<ISessionStore>().GetItemsAsync();
            foreach (var session in sessions)
                AppSettings.Remove("fav_" + session.Id);
        }

        public static bool LeftFeedback(string id) =>
            AppSettings.GetValueOrDefault("feed_"+id, false);

		public static bool LeftConferenceFeedback() =>
			AppSettings.GetValueOrDefault("conference_feedback", false);

		public static bool LeaveConferenceFeedback(bool leave) =>
			AppSettings.AddOrUpdateValue("conference_feedback", leave);

        public static void LeaveFeedback(string id, bool leave) =>
            AppSettings.AddOrUpdateValue("feed_"+id, leave);

        public static async Task ClearFeedback()
        {
            var sessions = await Locator.Get<ISessionStore>().GetItemsAsync();
            foreach (var session in sessions)
                AppSettings.Remove("feed_" + session.Id);
        }
    }
}