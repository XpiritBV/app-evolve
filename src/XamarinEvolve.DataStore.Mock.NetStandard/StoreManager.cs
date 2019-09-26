using XamarinEvolve.DataStore.Abstractions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Mock
{
	public class StoreManager : IStoreManager
	{
        private IDependencyService _locator;

        public StoreManager() : this(new DependencyServiceWrapper())
        {
            
        }

		public StoreManager(IDependencyService locator)
		{
            _locator = locator;
		}

		#region IStoreManager implementation

		public Task<bool> SyncAllAsync(bool syncUserSpecific)
		{
			return Task.FromResult(true);
		}

		public bool IsInitialized { get { return true; } }
		public Task<bool> InitializeAsync()
		{
			return Task.FromResult(true);
		}

		#endregion

		public Task DropEverythingAsync()
		{
			return Task.FromResult(true);
		}

		public Task<AccountResponse> LoginAnonymouslyAsync(string impersonateUserId = null)
		{
			return Task.FromResult(CreateMockAccountResponse());
		}

		public Task<AccountResponse> LoginAsync(string username, string password)
		{
			return Task.FromResult(CreateMockAccountResponse());
		}

		private AccountResponse CreateMockAccountResponse() =>
			new AccountResponse
			{
				Success = true,
				Token = "mock",
				User = new User
				{
					Email = "mock@mock.com",
					FirstName = "Mock",
					LastName = "User"
				}
			};

		public Task<bool> LogoutAsync()
		{
			return Task.FromResult(true);
		}

		INotificationStore notificationStore;
		public INotificationStore NotificationStore => notificationStore ?? (notificationStore = _locator.Get<INotificationStore>());

		IMiniHacksStore miniHacksStore;
		public IMiniHacksStore MiniHacksStore => miniHacksStore ?? (miniHacksStore = _locator.Get<IMiniHacksStore>());

		ICategoryStore categoryStore;
		public ICategoryStore CategoryStore => categoryStore ?? (categoryStore = _locator.Get<ICategoryStore>());

		IFavoriteStore favoriteStore;
		public IFavoriteStore FavoriteStore => favoriteStore ?? (favoriteStore = _locator.Get<IFavoriteStore>());

		IFeedbackStore feedbackStore;
		public IFeedbackStore FeedbackStore => feedbackStore ?? (feedbackStore = _locator.Get<IFeedbackStore>());

		IConferenceFeedbackStore conferenceFeedbackStore;
		public IConferenceFeedbackStore ConferenceFeedbackStore => conferenceFeedbackStore ?? (conferenceFeedbackStore = _locator.Get<IConferenceFeedbackStore>());

		ISessionStore sessionStore;
		public ISessionStore SessionStore => sessionStore ?? (sessionStore = _locator.Get<ISessionStore>());

		ISpeakerStore speakerStore;
		public ISpeakerStore SpeakerStore => speakerStore ?? (speakerStore = _locator.Get<ISpeakerStore>());

		IEventStore eventStore;
		public IEventStore EventStore => eventStore ?? (eventStore = _locator.Get<IEventStore>());

		ISponsorStore sponsorStore;
		public ISponsorStore SponsorStore => sponsorStore ?? (sponsorStore = _locator.Get<ISponsorStore>());

        IScavengerHuntStore scavengerHuntStore;
        public IScavengerHuntStore ScavengerHuntStore => scavengerHuntStore ?? (scavengerHuntStore = _locator.Get<IScavengerHuntStore>());

		IAttendeeStore attendeeStore;
		public IAttendeeStore AttendeeStore => attendeeStore ?? (attendeeStore = _locator.Get<IAttendeeStore>());
	}
}

