﻿using System;
using System.Globalization;

namespace XamarinEvolve.Utils
{
    public static class EventInfo
    {
        public const string EventShortName = "Techorama";
        public const string EventName = "Techorama";
        public static string VenueName => "Pathe Ede";
        public static string Address1 => "Laan der Verenigde Naties 150";
        public static string Address2 => "6716 JE Ede";
        public static string VenuePhoneNumber => "+31 20 549 1212";
        public static double Latitude => 52.015710d;
        public static double Longitude => 5.648140d;
        public static string TimeZoneName => "Europe/Amsterdam"; //https://en.wikipedia.org/wiki/List_of_tz_database_time_zones
        public static string HashTag => "#TechoramaNL";
        public static string WiFiSSIDDefault => "TechoramaNL";
        public static string WiFiPassDefault => "";
        public static string WifiUrl => AboutThisApp.CdnUrl + "wifi.json";

        public static string MiniHackStaffMemberName => "Mini-Hack coach";
        public static string MiniHackUnlockTag => "techorama";

        public static string TicketUrl => "https://techorama.nl/Tickets";
		public static string AudioStreamUrl => "https://web.interactio.io/event/1061";
		public static string AudioStreamEventCode => "\"Techorama\"";

		public static readonly CultureInfo Culture = new CultureInfo("nl-NL");

        public static readonly DateTime StartOfConference = new DateTime(2019, 9, 30, 6, 0, 0, DateTimeKind.Utc);
		public static readonly DateTime EndOfConference = new DateTime(2019, 10, 2, 20, 00, 0, DateTimeKind.Utc);
	}

	public static class AboutThisApp
	{
		public const string AppLinkProtocol = "techorama";
		public const string PackageName = "com.techorama.2019";
		public const string AppName = "Techorama";
		public const string CompanyName = "Xpirit";
		public const string Developer = "Xpirit";
		public static string DeveloperWebsite => "https://www.xpirit.com";
		public static string OpenSourceUrl => "http://xpir.it/tdappsource";
		public static string TermsOfUseUrl => "http://go.microsoft.com/fwlink/?linkid=206977";
		public static string PrivacyPolicyUrl => "https://www.techorama.nl/gdpr";
		public static string OpenSourceNoticeUrl => "http://xpir.it/ConferenceAppOSSLicenses";
		public static string EventRegistrationPage => "https://techorama.nl";
		public static string CdnUrl => "https://s3.eu-central-1.amazonaws.com/xpirit-techdays2017/"; 
		public const string AppLinksBaseDomain = "www.techorama.nl"; 
		public const string SessionsSiteSubdirectory = "agenda";
		public const string SpeakersSiteSubdirectory = "speakers";
		public const string SponsorsSiteSubdirectory = "partners";
		public const string MiniHacksSiteSubdirectory = "MiniHacks";
		public static string Copyright => "Copyright 2019 - Techorama";
		public static string CodeOfConductPageTitle => "Code of Conduct";

        public static string CustomVisionUrl => "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/3b0d84e6-08e7-4a85-91ee-79b1a352e772/image";
        public static string CustomVisionPredictionKey => "0ffd89f5454246f1abb8a0d4cae8aad8";
        public static float CustomVisionPredictionTreshold => 0.8f; // minimum probability for image predictions to be considered a match

        public const string Credits = "The Techorama 2019 mobile apps were handcrafted by Xpirit, based on the great work done by Xamarin.\n\n" +
			"Xpirit Team:\n" +
			"Roy Cornelissen\n" +
			"Marcel de Vries\n" +
			"Geert van der Cruijsen\n\n" +
			"Many thanks to the original Xamarin Team:\n" +
			"James Montemagno\n" +
			"Pierce Boggan\n" +
		   "\n" +
			"...and of course you! <3";        
    }

	public static class AppBehavior
	{
		public static readonly double RefreshIntervalInMinutes = 60 * 8; // 8 hours
		public static readonly double ShowPastSessionsTimeWindowInMinutes = 65;
		public static readonly int DelayBeforeSearchingInMilliseconds = 500;
		public static readonly int NumberOfDaysAfterConferenceToStopSyncing = 30; // after the conference, videos and slides may be added to sessions
		public const string EmailRegex = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
			@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
		public const int FloorLevelCorrection = -1;
		public const int MaxTotalAttemptsPerDay = 40;
		public const int ScavengerHuntCostPerAttempt = 5;
	}

	public static class PublicationSettings
	{
		public const uint iTunesAppId = 1475710427;
	}

	public static class ApiKeys
	{
		public const string ApiUrl = "https://techorama2019.azurewebsites.net";//"http://172.18.81.177:51800";		public const string HockeyAppiOS = "79ca52f47f774511890d1d1f6ff642fc";

		public const string GoogleAnalyticsTrackingId = "UA-82798885-1";

		public const string NotificationHubName = "techoramahub";
		public const string ListenConnectionString = "Endpoint=sb://confhub.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=tK78Onmf2nc9WLs+sGD97l5pRm/mvCf/kro5KHJdvyQ=";
		public static string[] NotificationHubTags = {  };
		public const string GoogleSenderId = "697994100298";

		public const string APNTemplateBody = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";
	}

	public static class MessageKeys
	{
		public const string NavigateToEvent = "navigate_event";
		public const string NavigateToSession = "navigate_session";
		public const string NavigateToSpeaker = "navigate_speaker";
		public const string NavigateToSponsor = "navigate_sponsor";
		public const string NavigateToImage = "navigate_image";
		public const string NavigateLogin = "navigate_login";
		public const string NavigateToSyncMobileToWebViewModel = "navigate_syncmobiletoweb";
		public const string NavigateToSyncWebToMobileViewModel = "navigate_syncwebtomobile";
		public const string NavigateToConferenceFeedback = "navigate_conferencefeedback";
		public const string NavigateToRoom = "navigate_room";
		public const string SessionFavoriteToggled = "session_favorite_toggled";
		public const string Error = "error";
		public const string Connection = "connection";
		public const string LoggedIn = "loggedin";
		public const string Message = "message";
		public const string Question = "question";
		public const string Choice = "choice";
	}
}

