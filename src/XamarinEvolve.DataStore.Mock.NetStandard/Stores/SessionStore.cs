using System;
using XamarinEvolve.DataStore.Abstractions;
using System.Threading.Tasks;
using System.Collections.Generic;
using XamarinEvolve.DataObjects;
using System.Linq;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Mock
{
    public class SessionStore : BaseStore<Session>,ISessionStore
    {
        List<Session> sessions;
        ISpeakerStore speakerStore;
        ICategoryStore categoryStore;
        IFavoriteStore favoriteStore;
        IFeedbackStore feedbackStore;

        public SessionStore (IDependencyService locator) : base (locator)
        {
		}

        public SessionStore() : this (new DependencyServiceWrapper())
        {
        }

        #region ISessionStore implementation

        public async override Task<Session> GetItemAsync(string id)
        {
			if (!initialized)
				await InitializeStore().ConfigureAwait(false);
            
            return sessions.FirstOrDefault(s => s.Id == id);
        }

        public async override Task<IEnumerable<Session>> GetItemsAsync(bool forceRefresh = false, Dictionary<string, string> param = null)
        {
            if (!initialized)
                await InitializeStore().ConfigureAwait(false);
            
            return sessions as IEnumerable<Session>;
        }

        public async Task<IEnumerable<Session>> GetSpeakerSessionsAsync(string speakerId)
        {
            if (!initialized)
                await InitializeStore().ConfigureAwait(false);
            
            var results =  from session in sessions
                           where session.StartTime.HasValue
                           orderby session.StartTime.Value
                           from speaker in session.Speakers
                           where speaker.Id == speakerId
                           select session;
            
            return results;
        }

        public async Task<IEnumerable<Session>> GetNextSessions(int maxNumber)
        {
            if (!initialized)
                await InitializeStore().ConfigureAwait(false);

			var referenceDate = Clock.Now.AddMinutes(-30);

            var results = (from session in sessions
			               where (session.IsFavorite && session.StartTime.HasValue && session.StartTime.Value.ToUniversalTime() > referenceDate)
                            orderby session.StartTime.Value
                            select session).Take(maxNumber);

            var enumerable = results as Session[] ?? results.ToArray();
            return !enumerable.Any() ? null : enumerable;
        }

        #endregion

        #region IBaseStore implementation
        bool initialized = false;
        public async override Task<bool> InitializeStore()
        {
            if (initialized)
                return true;

			speakerStore = Locator.Get<ISpeakerStore>();
			favoriteStore = Locator.Get<IFavoriteStore>();
			categoryStore = Locator.Get<ICategoryStore>();
			feedbackStore = Locator.Get<IFeedbackStore>();

			initialized = true;
            var categories = (await categoryStore.GetItemsAsync().ConfigureAwait(false)).ToArray();
            await speakerStore.InitializeStore().ConfigureAwait(false);
            var speakers = (await speakerStore.GetItemsAsync().ConfigureAwait(false)).ToArray();
            sessions = new List<Session>();
            int speaker = 0;
            int speakerCount = 0;
            int room = 0;
            int categoryCount = 0;
            int category = 0;
            var day = new DateTime(2017, 10, 14, 13, 0, 0, DateTimeKind.Utc);
            int dayCount = 0;
            for (int i = 0; i < titles.Length; i++)
            {
                var sessionSpeakers = new List<Speaker>();
                var sessionCategories = new List<Category>();

                categoryCount++;
                speakerCount++;
                
                for (int j = 0; j < speakerCount; j++)
                {
                    sessionSpeakers.Add(speakers[speaker]);
                    speaker++;
                    if (speaker >= speakers.Length)
                        speaker = 0;
                }

                if (i == 1)
                    sessionSpeakers.Add(sessions[0].Speakers.ElementAt(0));

                for (int j = 0; j < categoryCount; j++)
                {
                    sessionCategories.Add(categories[category]);
                    category++;
                    if (category >= categories.Length)
                        category = 0;
                }

                if (i == 1)
                    sessionCategories.Add(sessions[0].Categories.ElementAt(0));

                var rooms = RoomStore.Rooms;
                var ro = rooms[room];
                room++;
                if (room >= rooms.Length)
                    room = 0;

                sessions.Add(new Session
                {
                    Id = i.ToString(),
                    Abstract = "This is an abstract that is going to tell us all about how awsome this session is and that you should go over there right now and get ready for awesome!.",
                    Categories = sessionCategories,
                    Room = ro,
                    Speakers = sessionSpeakers,
                    Title = titles[i],
                    ShortTitle = titlesShort[i],
                    RemoteId = i.ToString(),
                    Level = ((i % 3 + 1) * 100).ToString(),
                    Language = (i % 3) == 0 ? "NL" : "EN"
                    });
                
                sessions[i].IsFavorite = await favoriteStore.IsFavorite(sessions[i].Id);
                sessions[i].FeedbackLeft = await feedbackStore.LeftFeedback(sessions[i]);

                SetStartEnd(sessions[i], day);

                if (i == titles.Length / 2)
                {
                    dayCount = 0;
                    day = new DateTime(2017, 10, 13, 13, 0, 0, DateTimeKind.Utc);
                }
                else
                {
                    dayCount++;
                    if (dayCount == 2)
                    {
                        day = day.AddHours(1);
                        dayCount = 0;
                    }
                }


                if (speakerCount > 2)
                    speakerCount = 0;
            }


            sessions.Add(new Session
                {
                    Id = sessions.Count.ToString(),
                    Abstract = "Coming soon",
                    Categories = categories.Take(1).ToList(),
                    Room = RoomStore.Rooms[0],
                    //Speakers = new List<Speaker>{ speakers[0] },
                    Title = "Something awesome!",
                    ShortTitle = "Awesome",
                });
            sessions[sessions.Count - 1].IsFavorite = await favoriteStore.IsFavorite(sessions[sessions.Count - 1].Id);
            sessions[sessions.Count - 1].FeedbackLeft = await feedbackStore.LeftFeedback(sessions[sessions.Count - 1]);
            sessions[sessions.Count - 1].StartTime = null;
            sessions[sessions.Count - 1].EndTime = null;
			return true;
        }

        void SetStartEnd(Session session, DateTime day)
        {
            session.StartTime = day;
            session.EndTime = session.StartTime.Value.AddHours(1);
        }

        public Task<Session> GetAppIndexSession (string id)
        {
            return GetItemAsync (id);
        }

        public Task<IEnumerable<Session>> GetRoomSessions(string roomId)
        {
            return Task.FromResult(sessions.Where(s => s.Room?.Id == roomId));
        }        

        string[] titles = {
            "Create stunning apps with the Xamarin Designer for iOS",
            "Everyone can create beautiful apps with material design",
            "Dispelling design myths and making apps better",
            "3 Platforms: 1 codebase—your first Xamarin.Forms app",
            "Mastering XAML in Xamarin.Forms",
            "NuGet your code to all the platforms with portable class libraries",
            "A new world of possibilities for contextual awareness with iBeacons",
            "Wearables and IoT: Taking C# with you everywhere",
            "Create the next great mobile app in a weekend",
            "Best practices for effective iOS memory management",
            "Navigation design patterns for iOS and Android",
            "Is your app secure?",
            "Introduction to Xamarin.Insights",
            "Cross platform unit testing with xUnit",
            "Test automation in practice with Xamarin Test Cloud at MixRadio",
            "Why you should be building better mobile apps with reactive programming",
            "Create your own sci-fi with mobile augmented reality",
            "Addressing the OWASP mobile security threats using Xamarin"

        };

        string[] titlesShort = {
            "Stunning iOS Apps",
            "Material Design",
            "Making apps better",
            "3 Platforms: 1 codebase",
            "Mastering XAML",
            "NuGet your code",
            "iBeacons",
            "Wearables and IoT",
            "The next great app",
            "iOS Best Practices",
            "Navigation patterns",
            "Is your app secure?",
            "Xamarin.Insights",
            "xUnit",
            "Test Cloud at MixRadio",
            "Reactive programming",
            "Augmented reality",
            "OWASP mobile security"
        };

        #endregion
    }
}

