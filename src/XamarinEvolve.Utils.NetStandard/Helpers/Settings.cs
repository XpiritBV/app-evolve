using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using XamarinEvolve.Clients.Portable;

namespace XamarinEvolve.Utils
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public class Settings : INotifyPropertyChanged
    {
		private static ISettings _current = CrossSettings.Current;
		private readonly IAppVersionProvider _appVersionProvider;

        public static void SetProviders(ISettings provider, IDependencyService dependencyService)
		{
			_current = provider;
            _instance = new Settings(dependencyService);
		}

		static ISettings AppSettings
		{
			get
			{
				return _current;
			}
		}

        static Settings _instance;

        /// <summary>
        /// Gets or sets the current settings. This should always be used
        /// </summary>
        /// <value>The current.</value>
        public static Settings Current
        {
            get { return _instance ?? (_instance = new Settings()); }
        }

		private IPlatformSpecificSettings _platformSettings;
        private readonly IDependencyService _locator;

        public Settings() : this (new DependencyServiceWrapper())
		{
		}

		public Settings(IDependencyService locator)
        {
            _locator = locator;
			_appVersionProvider = _locator.Get<IAppVersionProvider>();
            _platformSettings = _locator.Get<IPlatformSpecificSettings>();
		}

		const string WiFiSSIDKey = "ssid_key";
        public string WiFiSSID 
        {
            get { return AppSettings.GetValueOrDefault (WiFiSSIDKey, EventInfo.WiFiSSIDDefault); }
            set 
            {
                if (AppSettings.AddOrUpdateValue (WiFiSSIDKey, value)) 
                {
                    OnPropertyChanged ();
                }
            }
        }

        const string WiFiPassKey = "wifi_pass_key";
        public string WiFiPass 
        {
            get { return AppSettings.GetValueOrDefault (WiFiPassKey, EventInfo.WiFiPassDefault); }
            set 
            {
                if (AppSettings.AddOrUpdateValue (WiFiPassKey, value)) 
                {
                    OnPropertyChanged ();
                }
            }
        }

        public void UnlockScavengerHuntObject(string id, bool value = true)
        {
			AppSettings.AddOrUpdateValue("scavengerhuntobject_" + id, value);
        }

		public bool IsScavengerHuntObjectUnlocked(string id)
		{
			return AppSettings.GetValueOrDefault("scavengerhuntobject_" + id, false);
		}

		public void SaveReminderId(string id, string calId)
        {
            AppSettings.AddOrUpdateValue(GetReminderId(id), calId);
        }

        string GetReminderId(string id)
        {
            return "reminder_" + id;
        }

        public string GetEventId(string id)
        {
            return AppSettings.GetValueOrDefault(GetReminderId(id), string.Empty);
        }

        public void RemoveReminderId(string id)
        {
            AppSettings.Remove(GetReminderId(id));
        }

		public void LeaveConferenceFeedback()
		{
			AppSettings.AddOrUpdateValue("conferencefeedback_finished", true);
		}

		public bool IsConferenceFeedbackFinished()
		{
			return AppSettings.GetValueOrDefault("conferencefeedback_finished", false);
		}

		public bool IsHackFinished(string id)
        {
            return AppSettings.GetValueOrDefault("minihack_" + id, false);
        }

        public void FinishHack(string id, bool value = true)
        {
            AppSettings.AddOrUpdateValue("minihack_" + id, value);
        }

        const string LastFavoriteTimeKey = "last_favorite_time";

        public DateTime LastFavoriteTime
        {
            get { return AppSettings.GetValueOrDefault(LastFavoriteTimeKey, Clock.Now); }
            set
            {
                AppSettings.AddOrUpdateValue(LastFavoriteTimeKey, value);
            }
        }

        const string HasSetReminderKey = "set_a_reminder";
        static readonly bool HasSetReminderDefault = false;

        public bool HasSetReminder
        {
            get { return AppSettings.GetValueOrDefault(HasSetReminderKey, HasSetReminderDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(HasSetReminderKey, value);
            }
        }

		const string EventCalendarKey = "event_calendar";
		static readonly string EventCalendarIdDefault = string.Empty;
        public string EventCalendarId
        {
            get { return AppSettings.GetValueOrDefault(EventCalendarKey, EventCalendarIdDefault); }
            set { AppSettings.AddOrUpdateValue(EventCalendarKey, value); }
        }
          

        const string PushNotificationsEnabledKey = "push_enabled";
        static readonly bool PushNotificationsEnabledDefault = false;

        public bool PushNotificationsEnabled
        {
            get { return AppSettings.GetValueOrDefault(PushNotificationsEnabledKey, PushNotificationsEnabledDefault); }
            set
            {
                if (AppSettings.AddOrUpdateValue(PushNotificationsEnabledKey, value))
                    OnPropertyChanged();
            }
        }

        const string FirstRunKey = "first_run";
        static readonly bool FirstRunDefault = true;

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see favorites only.
        /// </summary>
        /// <value><c>true</c> if favorites only; otherwise, <c>false</c>.</value>
        public bool FirstRun
        {
            get { return AppSettings.GetValueOrDefault(FirstRunKey, FirstRunDefault); }
            set
            {
                if (AppSettings.AddOrUpdateValue(FirstRunKey, value))
                    OnPropertyChanged();
            }
        }

        const string GooglePlayCheckedKey = "play_checked";
        static readonly bool GooglePlayCheckedDefault = false;

        public bool GooglePlayChecked
        {
            get { return AppSettings.GetValueOrDefault(GooglePlayCheckedKey, GooglePlayCheckedDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(GooglePlayCheckedKey, value);
            }
        }

        const string AttemptedPushKey = "attempted_push";
        static readonly bool AttemptedPushDefault = false;

        public bool AttemptedPush
        {
            get { return AppSettings.GetValueOrDefault(AttemptedPushKey, AttemptedPushDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(AttemptedPushKey, value);
            }
        }

       
        const string PushRegisteredKey = "push_registered";
        static readonly bool PushRegisteredDefault = false;

        public bool PushRegistered
        {
            get { return AppSettings.GetValueOrDefault(PushRegisteredKey, PushRegisteredDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(PushRegisteredKey, value);
            }
        }

        const string FavoriteModeKey = "favorites_only";
        static readonly bool FavoriteModeDefault = false;

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to see favorites only.
        /// </summary>
        /// <value><c>true</c> if favorites only; otherwise, <c>false</c>.</value>
        public bool FavoritesOnly
        {
            get { return AppSettings.GetValueOrDefault(FavoriteModeKey, FavoriteModeDefault); }
            set
            {
                if (AppSettings.AddOrUpdateValue(FavoriteModeKey, value))
                    OnPropertyChanged();
            }
        }

		const string ScavengerHuntAttemptsKey = "scavengerhunt_attempts_";

		public int GetScavengerHuntAttempts(string objectId)
		{
			return AppSettings.GetValueOrDefault(ScavengerHuntAttemptsKey + objectId, 0);
		}

		public int GetScavengerHuntAttemptsToday()
		{
			var key = $"{ScavengerHuntAttemptsKey}{Clock.Now.DayOfYear}{Clock.Now.Year}";
			return AppSettings.GetValueOrDefault(key, 0);
		}

		public void IncrementScavengerHuntAttempt(string objectId)
		{
			var currentValue = GetScavengerHuntAttempts(objectId);
			AppSettings.AddOrUpdateValue(ScavengerHuntAttemptsKey + objectId, currentValue + 1);

			var dayKey = $"{ScavengerHuntAttemptsKey}{Clock.Now.DayOfYear}{Clock.Now.Year}";
			var currentDayValue = GetScavengerHuntAttemptsToday();
			AppSettings.AddOrUpdateValue(dayKey, currentDayValue + 1);
		}

		public void ResetScavengerHuntAttempts(string objectId)
		{
			AppSettings.AddOrUpdateValue(ScavengerHuntAttemptsKey + objectId, 0);
			var dayKey = $"{ScavengerHuntAttemptsKey}{Clock.Now.DayOfYear}{Clock.Now.Year}";
			AppSettings.AddOrUpdateValue(dayKey, 0);
		}

		const string ShowAllCategoriesKey = "all_categories";
        static readonly bool ShowAllCategoriesDefault = true;

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to show all categories.
        /// </summary>
        /// <value><c>true</c> if show all categories; otherwise, <c>false</c>.</value>
        public bool ShowAllCategories
        {
            get { return AppSettings.GetValueOrDefault(ShowAllCategoriesKey, ShowAllCategoriesDefault); }
            set
            {
                if (AppSettings.AddOrUpdateValue(ShowAllCategoriesKey, value))
                    OnPropertyChanged();
            }
        }

        const string ShowPastSessionsKey = "show_past_sessions";
        static readonly bool ShowPastSessionsDefault = false;

		/// <summary>
		/// Gets or sets a value indicating whether the user wants show past sessions.
		/// </summary>
		/// <value><c>true</c> if show past sessions; otherwise, <c>false</c>.</value>
		public bool ShowPastSessions
        {
            get 
            {
                //if end of conference
				if (Clock.Now > EventInfo.EndOfConference)
                    return true;
                
                return AppSettings.GetValueOrDefault(ShowPastSessionsKey, ShowPastSessionsDefault); 
            }
            set
            {
                if (AppSettings.AddOrUpdateValue(ShowPastSessionsKey, value))
                    OnPropertyChanged();
            }
        }

        const string FilteredCategoriesKey = "filtered_categories";
        static readonly string FilteredCategoriesDefault = string.Empty;

        public string FilteredCategories
        {
            get { return AppSettings.GetValueOrDefault(FilteredCategoriesKey, FilteredCategoriesDefault); }
            set
            {
                if (AppSettings.AddOrUpdateValue(FilteredCategoriesKey, value))
                    OnPropertyChanged();
            }
        }

		const string EmailKey = "email_key";
		readonly string EmailDefault = string.Empty;
		public string Email
		{
			get { return AppSettings.GetValueOrDefault(EmailKey, EmailDefault); }
			set
			{
				if (AppSettings.AddOrUpdateValue(EmailKey, value))
				{
					OnPropertyChanged();
					OnPropertyChanged(nameof(UserAvatar));
				}
			}
		}

		const string UserIdentifierKey = "useridentifier_key";
        readonly string UserIdentifierDefault = string.Empty;
        public string UserIdentifier 
        {
            get 
			{
				var id = AppSettings.GetValueOrDefault(UserIdentifierKey, UserIdentifierDefault);

				if (_platformSettings != null && _platformSettings.UserIdentifier != id)
				{
					_platformSettings.UserIdentifier = id;
				}
				return id;
			}
            set
            {
                if (AppSettings.AddOrUpdateValue(UserIdentifierKey, value))
                {
					if (_platformSettings != null)
					{
						_platformSettings.UserIdentifier = value;
					}
                    OnPropertyChanged();
                }
            }
        }

        const string DatabaseIdKey = "azure_database";
        static readonly int DatabaseIdDefault = 0;

        public static int DatabaseId
        {
            get { return AppSettings.GetValueOrDefault(DatabaseIdKey, DatabaseIdDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(DatabaseIdKey, value);
            }
        }

        public static int UpdateDatabaseId()
        {
            return DatabaseId++;
        }

        const string FirstNameKey = "firstname_key";
        readonly string FirstNameDefault =  string.Empty;
        public string FirstName 
        {
            get { return AppSettings.GetValueOrDefault(FirstNameKey, FirstNameDefault); }
            set
            {
                if (AppSettings.AddOrUpdateValue(FirstNameKey, value))
                {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UserDisplayName));
                }
            }
        }

        const string LastNameKey = "lastname_key";
        readonly string LastNameDefault =  string.Empty;
        public string LastName 
        {
            get { return AppSettings.GetValueOrDefault(LastNameKey, LastNameDefault); }
            set
            {
                if (AppSettings.AddOrUpdateValue(LastNameKey, value))
                {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UserDisplayName));
                }
            }
        }


        public bool NeedsSync => string.Compare(LastKnownAppVersion, _appVersionProvider.AppVersion) < 0 
		                               || !HasSyncedData 
		                               || LastSync < Clock.Now.AddDays(-1); 

		const string LastKnownAppVersionKey = "last_known_app_version";
		const string LastKnownAppVersionDefault = "0";
		public string LastKnownAppVersion
		{
			get
			{
				return AppSettings.GetValueOrDefault(LastKnownAppVersionKey, LastKnownAppVersionDefault);
			}
			set
			{
				AppSettings.AddOrUpdateValue(LastKnownAppVersionKey, value);
			}
		}

		const string LoginAttemptsKey = "login_attempts";
        const int LoginAttemptsDefault = 0;
        public int LoginAttempts
        {
            get
            {
                return AppSettings.GetValueOrDefault(LoginAttemptsKey, LoginAttemptsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(LoginAttemptsKey, value);
            }
        }

        const string HasSyncedDataKey = "has_synced";
        const bool HasSyncedDataDefault = false;
        public bool HasSyncedData
        {
            get { return AppSettings.GetValueOrDefault(HasSyncedDataKey, HasSyncedDataDefault); }
            set { AppSettings.AddOrUpdateValue(HasSyncedDataKey, value); }

        }

        const string LastSyncKey = "last_sync";
        static readonly DateTime LastSyncDefault = Clock.Now.AddDays(-30);
        public DateTime LastSync
        {
            get
            {
                return AppSettings.GetValueOrDefault(LastSyncKey, LastSyncDefault);
            }
            set
            {
				if (AppSettings.AddOrUpdateValue(LastSyncKey, value))
				{
					LastKnownAppVersion = _appVersionProvider.AppVersion;
					OnPropertyChanged();
				}
            }
        } 

        bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set 
            { 
                if (isConnected == value)
                    return;
                isConnected = value;
                OnPropertyChanged();
            }
        }

		#region Helpers


		public string UserDisplayName => IsLoggedIn ? $"{FirstName} {LastName}" : "Sign In";

        public string UserAvatar => IsLoggedIn ? Gravatar.GetURL(Email) : "profile_generic.png";

		public bool IsLoggedIn => !string.IsNullOrWhiteSpace(UserIdentifier);

        public bool HasFilters => (ShowPastSessions || FavoritesOnly || (!string.IsNullOrWhiteSpace(FilteredCategories) && !ShowAllCategories));

		#endregion

		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}