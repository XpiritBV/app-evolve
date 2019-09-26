namespace XamarinEvolve.Utils
{
	public static class FeatureFlags
	{
		public static bool LoginEnabled => false; // Set to true to use original login provider; false means that anonymous accounts are used
		public static bool ManualSyncEnabled => false;
		public static bool MiniHacksEnabled => false;
		public static bool EventsEnabled => false;
		public static bool ShowBuyTicketButton => true;
		public static bool ShowConferenceFeedbackButton => true;
		public static bool SpeakersEnabled => true;
        public static bool SponsorsOnTabPage => false;
		public static bool ScavengerHuntsEnabled => false;

		public static bool WifiEnabled => false;
		public static bool EvalEnabled => false;
		public static bool CodeOfConductEnabled => true;
        public static bool FloormapEnabled => true;
		public static bool AppLinksEnabled => false;
		public static bool ConferenceInformationEnabled => false;
		public static bool AppToWebLinkingEnabled => true;
		public static bool ShowLocationInSessionCell => true;

        public static bool UseMocks => false;
		public static bool HockeyAppEnabled => false; // disabled HockeyApp to reduce overhead
		public static bool GoogleAnalyticsEnabled => true;

		public static bool AudioStreamEnabled => false;
	}
}

