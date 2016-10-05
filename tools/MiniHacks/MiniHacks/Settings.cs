using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace MiniHacks.Helpers
{
    public static class Settings
    {
        static ISettings AppSettings => CrossSettings.Current;

        public static string JsonFile
        {
            get { return AppSettings.GetValueOrDefault<string>("json", string.Empty); }
            set { AppSettings.AddOrUpdateValue<string>("json", value); }
        }

        public static int GetCount(string id) => 
            AppSettings.GetValueOrDefault<int>(id, 0);


        public static void UpdateCount(string id) =>
            AppSettings.AddOrUpdateValue<int>(id, GetCount(id) + 1);

    }
}