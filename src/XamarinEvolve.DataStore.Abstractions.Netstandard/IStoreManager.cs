using System.Threading.Tasks;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.DataStore.Abstractions
{
	public interface IStoreManager
	{
		bool IsInitialized { get; }
		Task<bool> InitializeAsync();
		ICategoryStore CategoryStore { get; }
		IFavoriteStore FavoriteStore { get; }
		IFeedbackStore FeedbackStore { get; }
		IConferenceFeedbackStore ConferenceFeedbackStore { get; }
		ISessionStore SessionStore { get; }
		ISpeakerStore SpeakerStore { get; }
		ISponsorStore SponsorStore { get; }
		IEventStore EventStore { get; }
		IMiniHacksStore MiniHacksStore { get; }
        IScavengerHuntStore ScavengerHuntStore { get; }
		INotificationStore NotificationStore { get; }
		IAttendeeStore AttendeeStore { get; }

		Task<bool> SyncAllAsync(bool syncUserSpecific);
		Task DropEverythingAsync();

		Task<AccountResponse> LoginAnonymouslyAsync(string impersonateUserId = null);
		Task<AccountResponse> LoginAsync(string username, string password);
		Task<bool> LogoutAsync();
	}
}

