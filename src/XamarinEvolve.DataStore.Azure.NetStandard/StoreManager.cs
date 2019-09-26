using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using System.Collections.Generic;
using System.Diagnostics;
using XamarinEvolve.Utils;
using System.Threading;
using HttpTracing;
using System.Net.Http;
using XamarinEvolve.Clients.Portable.NetStandard.Helpers;

namespace XamarinEvolve.DataStore.Azure
{
    public class StoreManager : IStoreManager
    {
        private IDependencyService _locator;
		private static SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1);
		HttpEventListener listener = new HttpEventListener();

		public StoreManager() : this(new DependencyServiceWrapper())
        {
            
        }

        public StoreManager(IDependencyService locator)
		{
            _locator = locator;
		}

		public static MobileServiceClient MobileService { get; set; }

        /// <summary>
        /// Syncs all tables.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="syncUserSpecific">If set to <c>true</c> sync user specific.</param>
        public async Task<bool> SyncAllAsync(bool syncUserSpecific)
        {
            await new SynchronizationContextRemover(); // force background thread
		

			if (!IsInitialized)
            {
                await InitializeAsync();
			}

            var taskList = new List<Task<bool>>
            {
                CategoryStore.SyncAsync(),
                NotificationStore.SyncAsync(),
                SpeakerStore.SyncAsync(),
                SessionStore.SyncAsync(),
                SponsorStore.SyncAsync(),
                RoomStore.SyncAsync(),
            };

            if (FeatureFlags.MiniHacksEnabled)
            {
                taskList.Add(MiniHacksStore.SyncAsync());
            }
            if (FeatureFlags.ScavengerHuntsEnabled)
            {
                taskList.Add(ScavengerHuntStore.SyncAsync());
				if (syncUserSpecific)
				{
					taskList.Add(AttendeeStore.SyncAsync());
				}
			}
            if (FeatureFlags.EventsEnabled)
            {
                taskList.Add(EventStore.SyncAsync());
            }

            if (syncUserSpecific)
            {
                taskList.Add(FeedbackStore.SyncAsync());
                taskList.Add(FavoriteStore.SyncAsync());
                taskList.Add(ConferenceFeedbackStore.SyncAsync());
            }

            var successes = await Task.WhenAll(taskList).ConfigureAwait(false);
			var successful = !successes.Any(x => !x);
			Debug.WriteLine($"Done with all sync tasks; Success: {successful}");

            //if (successful)
            //{
            //    await IndexSessions().ConfigureAwait(false);
            //    await IndexSpeakers().ConfigureAwait(false);
            //    await IndexMiniHacks().ConfigureAwait(false);
            //}

            return successful;
        }

        private async Task IndexSessions()
        {
			var dataIndexer = _locator.Get<IDataIndexer>();
			var dataShare = _locator.Get<IPlatformSpecificDataHandler<Session>>();

			if (dataIndexer == null && dataShare == null)
				return; // don't bother

			var sessions = await SessionStore.GetItemsAsync().ConfigureAwait(false);

            if (dataIndexer != null && dataIndexer.IsSupported())
            {
                foreach (var session in sessions)
                {
                    await dataIndexer.RegisterSession(session);
                }
            }

			if (dataShare != null)
			{
				await dataShare.UpdateMultipleEntities(sessions);
			}
		}

		private async Task IndexSpeakers()
		{
			var dataIndexer = _locator.Get<IDataIndexer>();
			if (dataIndexer == null)
				return; // don't bother

			if (dataIndexer.IsSupported())
			{
				var speakers = await SpeakerStore.GetItemsAsync().ConfigureAwait(false);

				foreach (var speaker in speakers)
				{
                    await dataIndexer.RegisterSpeaker(speaker);
				}
			}
		}

		private async Task IndexMiniHacks()
		{
            if (!FeatureFlags.MiniHacksEnabled) return;

			var dataIndexer = _locator.Get<IDataIndexer>();
			if (dataIndexer == null)
				return; // don't bother

			if (dataIndexer.IsSupported())
			{
				var hacks = await MiniHacksStore.GetItemsAsync().ConfigureAwait(false);

				foreach (var hack in hacks)
				{
                    await dataIndexer.RegisterMiniHack(hack);
				}
			}
		}

		/// <summary>
		/// Drops all tables from the database and updated DB Id
		/// </summary>
		/// <returns>The everything async.</returns>
		public Task DropEverythingAsync()
        {
            Settings.UpdateDatabaseId();
            CategoryStore.DropTable();
            EventStore.DropTable();
            MiniHacksStore.DropTable();
            NotificationStore.DropTable();
            SessionStore.DropTable();
            RoomStore.DropTable();
            SpeakerStore.DropTable();
            SponsorStore.DropTable();
            FeedbackStore.DropTable();
            FavoriteStore.DropTable();
			ConferenceFeedbackStore.DropTable();
            ScavengerHuntStore.DropTable();
			AttendeeStore.DropTable();
            IsInitialized = false;
            return Task.FromResult(true);
        }

        public bool IsInitialized { get; private set; }

        #region IStoreManager implementation
        public async Task<bool> InitializeAsync()
        {
            MobileServiceSQLiteStore store;
			bool result;
			await _initializationSemaphore.WaitAsync();

			try
			{
				if (IsInitialized)
				{
					return true ;
				}

				var dbId = Settings.DatabaseId;
				var path = $"syncstore{dbId}.db";

				var handlerProvider = _locator.Get<IMessageHandlerProvider>();
				var authHandler = new MobileServicesAuthenticationHandler();
				if (handlerProvider != null)
				{
				    MobileService = new MobileServiceClient(ApiKeys.ApiUrl, authHandler, handlerProvider.GetHandler());
				}
				else
				{
					MobileService = new MobileServiceClient(ApiKeys.ApiUrl, authHandler);
				}
				authHandler.Client = MobileService;

				MobileService.SerializerSettings.CamelCasePropertyNames = false;

				store = new MobileServiceSQLiteStore (path);
				store.DefineTable<Category> ();
				store.DefineTable<Favorite> ();
				store.DefineTable<Notification> ();
				store.DefineTable<FeaturedEvent> ();
				store.DefineTable<Feedback> ();
				store.DefineTable<Room> ();
				store.DefineTable<Session> ();
				store.DefineTable<Speaker> ();
				store.DefineTable<Sponsor> ();
				store.DefineTable<SponsorLevel> ();
				store.DefineTable<StoreSettings> ();
				store.DefineTable<MiniHack> ();
				store.DefineTable<ConferenceFeedback> ();
				store.DefineTable<ScavengerHunt> ();
				store.DefineTable<Attendee> ();

				await MobileService.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler()).ConfigureAwait(false);
				IsInitialized = true;
				result = await LoadCachedTokenAsync().ConfigureAwait(false);
			}
            catch(Exception e)
            {
                Debug.WriteLine(e);
                Debugger.Break();
				return false;
            }
			finally
			{
				_initializationSemaphore.Release();
			}
			return result;
        }

        IMiniHacksStore miniHacksStore;
        public IMiniHacksStore MiniHacksStore => miniHacksStore ?? (miniHacksStore  = _locator.Get<IMiniHacksStore>());
       
        INotificationStore notificationStore;
        public INotificationStore NotificationStore => notificationStore ?? (notificationStore  = _locator.Get<INotificationStore>());

        ICategoryStore categoryStore;
        public ICategoryStore CategoryStore => categoryStore ?? (categoryStore  = _locator.Get<ICategoryStore>());

        IFavoriteStore favoriteStore;
        public IFavoriteStore FavoriteStore => favoriteStore ?? (favoriteStore  = _locator.Get<IFavoriteStore>());

        IFeedbackStore feedbackStore;
        public IFeedbackStore FeedbackStore => feedbackStore ?? (feedbackStore  = _locator.Get<IFeedbackStore>());

		IConferenceFeedbackStore conferenceFeedbackStore;
		public IConferenceFeedbackStore ConferenceFeedbackStore => conferenceFeedbackStore ?? (conferenceFeedbackStore = _locator.Get<IConferenceFeedbackStore>());

        ISessionStore sessionStore;
        public ISessionStore SessionStore => sessionStore ?? (sessionStore  = _locator.Get<ISessionStore>());

        IRoomStore roomStore;
        public IRoomStore RoomStore => roomStore ?? (roomStore = _locator.Get<IRoomStore>());

		ISpeakerStore speakerStore;
        public ISpeakerStore SpeakerStore => speakerStore ?? (speakerStore  = _locator.Get<ISpeakerStore>());

        IEventStore eventStore;
        public IEventStore EventStore => eventStore ?? (eventStore = _locator.Get<IEventStore>());

        ISponsorStore sponsorStore;
        public ISponsorStore SponsorStore => sponsorStore ?? (sponsorStore = _locator.Get<ISponsorStore>());

        IScavengerHuntStore scavengerHuntStore;
        public IScavengerHuntStore ScavengerHuntStore => scavengerHuntStore ?? (scavengerHuntStore = _locator.Get<IScavengerHuntStore>());

		IAttendeeStore attendeeStore;
		public IAttendeeStore AttendeeStore => attendeeStore ?? (attendeeStore = _locator.Get<IAttendeeStore>());

		#endregion

		public async Task<AccountResponse> LoginAnonymouslyAsync(string impersonateUserId = null)
		{
			if (!IsInitialized)
			{
                await InitializeAsync().ConfigureAwait(false);
			}

			if (string.IsNullOrEmpty(impersonateUserId))
			{
				var settings = await ReadSettingsAsync().ConfigureAwait(false);
				impersonateUserId = settings?.UserId; // see if we have a saved user id from a previous token
			}

			var credentials = new JObject();
			if (!string.IsNullOrEmpty(impersonateUserId))
			{
				credentials["anonymousUserId"] = impersonateUserId;
			}
			MobileServiceUser user = await MobileService.LoginAsync("AnonymousUser", credentials).ConfigureAwait(false);

			await CacheToken(user).ConfigureAwait(false);

			return user.AsAccount();
		}

        public async Task<AccountResponse> LoginAsync(string username, string password)
        {
            if (!IsInitialized)
            {
                await InitializeAsync().ConfigureAwait(false);
            }

            var credentials = new JObject();
            credentials["email"] = username;
            credentials["password"] = password;

            MobileServiceUser user = await MobileService.LoginAsync("Xamarin", credentials).ConfigureAwait(false);

            await CacheToken(user).ConfigureAwait(false);

			return user.AsAccount();
        }

        public async Task<bool> LogoutAsync()
        {
			bool result = true;
            if (!IsInitialized)
            {
                result = await InitializeAsync().ConfigureAwait(false);
            }

            await MobileService.LogoutAsync().ConfigureAwait(false);

            var settings = await ReadSettingsAsync().ConfigureAwait(false);

            if (settings != null)
            {
                settings.AuthToken = string.Empty;
                settings.UserId = string.Empty;

               result = await SaveSettingsAsync(settings).ConfigureAwait(false);
            }
			return result;
        }

        async Task<bool> SaveSettingsAsync(StoreSettings settings)
        {
            await MobileService.SyncContext.Store.UpsertAsync(nameof(StoreSettings), new[] { JObject.FromObject(settings) }, true);
			return true;
		}

        async Task<StoreSettings> ReadSettingsAsync()
        {
            return (await MobileService.SyncContext.Store.LookupAsync(nameof(StoreSettings), StoreSettings.StoreSettingsId).ConfigureAwait(false))?.ToObject<StoreSettings>();
        }

        async Task<bool> CacheToken(MobileServiceUser user)
        {
            var settings = new StoreSettings
            {
                UserId = user.UserId,
                AuthToken = user.MobileServiceAuthenticationToken
            };

            return await SaveSettingsAsync(settings).ConfigureAwait(false);            
        }

        async Task<bool> LoadCachedTokenAsync()
        {
            StoreSettings settings = await ReadSettingsAsync().ConfigureAwait(false);

            if (settings != null)
            {
                try
                {
					if (!string.IsNullOrEmpty(settings.AuthToken)
						&& JwtUtility.GetTokenExpiration(settings.AuthToken) > Clock.Now
						&& JwtUtility.IsIntendedForAudience(settings.AuthToken, ApiKeys.ApiUrl))
					{
						MobileService.CurrentUser = new MobileServiceUser(settings.UserId);
						MobileService.CurrentUser.MobileServiceAuthenticationToken = settings.AuthToken;
					}
					else
					{
						await ClearOldCredentials(settings).ConfigureAwait(false);
					}
                }
                catch (InvalidTokenException)
				{
					await ClearOldCredentials(settings).ConfigureAwait(false);
					return false;
				}
			}
			return true;
		}

		public class StoreSettings
        {
            public const string StoreSettingsId = "store_settings";

            public StoreSettings()
            {
                Id = StoreSettingsId;
            }

            public string Id { get; set; }

            public string UserId { get; set; }

            public string AuthToken { get; set; }
        }

		async Task ClearOldCredentials(StoreSettings settings)
		{
			if (FeatureFlags.LoginEnabled)
			{
				// if anonymous login is used, we must keep original user ID around to be used later
				// when using non-anonymous login, the user will provide their user ID via login
				settings.UserId = string.Empty;
			}
			settings.AuthToken = string.Empty;
			Settings.Current.UserIdentifier = string.Empty; // triggers a new login once we hit an authenticated API

			await SaveSettingsAsync(settings).ConfigureAwait(false);
		}
    }
}

