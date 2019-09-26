using System;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Mock
{
    public class EventStore : BaseStore<FeaturedEvent>, IEventStore
    {
        List<FeaturedEvent> Events { get; }
        ISponsorStore sponsors;
        public EventStore()
        {
            Events = new List<FeaturedEvent>();
            sponsors = Locator.Get<ISponsorStore>();
        }

        public override async Task<bool> InitializeStore()
        {
            if (Events.Count != 0)
                return true;

            var sponsorList = await sponsors.GetItemsAsync();

            
            Events.Add(new FeaturedEvent
                {
                    Title = "Registration for Training & System Config",
                    Description = "Get ready for TechDays training with open registration and full system configuration prep throughout the day!",
                    StartTime = new DateTime(2017, 10, 12, 16, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                    LocationName = "Registration",
                    IsAllDay = false,
                });
            
            Events.Add(new FeaturedEvent
                {
                    Title = "Training Keynote",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 1, 30, 0, DateTimeKind.Utc),
                    LocationName = "General Session",
                    IsAllDay = false,
                });

            Events.Add(new FeaturedEvent
                {
                    Title = "Breakfast",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 11, 30, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 13, 00, 0, DateTimeKind.Utc),
                    LocationName = "Meals",
                    IsAllDay = false,
                });

            Events.Add(new FeaturedEvent
                {
                    Title = "Training Day 1",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 13, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 22, 00, 0, DateTimeKind.Utc),
                    LocationName = "Training Breakouts",
                    IsAllDay = false,
                });

            Events.Add(new FeaturedEvent
                {
                    Title = "Evening Event",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 23, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 1, 0, 0, DateTimeKind.Utc),
                    LocationName = string.Empty,
                    IsAllDay = false,
                });

            Events.Add(new FeaturedEvent
                {
                    Title = "Breakfast",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 11, 30, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 13, 0, 0, DateTimeKind.Utc),
                    LocationName = "Meals",
                    IsAllDay = false,
                });

            Events.Add(new FeaturedEvent
                {
                    Title = "Training Day 2",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 13, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 22, 0, 0, DateTimeKind.Utc),
                    LocationName = "Training Breakouts",
                    IsAllDay = false,
                });

            Events.Add (new FeaturedEvent {
                Title = "Darwin Lounge",
                Description = "Stocked full of tech toys, massage chairs, Xamarin engineers, and a few really cool surprises, the Darwin Lounge is your place to hang out and relax between sessions. Back by popular demand, we’ll have several code challenges—Mini-Hacks—for you to complete to earn awesome prizes.",
                StartTime = new DateTime (2017, 10, 13, 23, 00, 0, DateTimeKind.Utc),
                EndTime = new DateTime (2017, 10, 14, 4, 0, 0, DateTimeKind.Utc),
                LocationName = "Darwin Lounge",
                IsAllDay = false,
            });


            Events.Add(new FeaturedEvent
                {
                    Title = "Conference Registration",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 13, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 23, 0, 0, DateTimeKind.Utc),
                    LocationName = "Registration",
                    IsAllDay = false,
                });

            Events.Add(new FeaturedEvent
                {
                    Title = "Evening Event",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 23, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 14, 1, 0, 0, DateTimeKind.Utc),
                LocationName = string.Empty,
                    IsAllDay = false,
                });

            Events.Add(new FeaturedEvent
                {
                    Title = "Breakfast",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 14, 12, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 14, 13, 0, 0, DateTimeKind.Utc),
                    LocationName = "Meals",
                    IsAllDay = false,
                });


            Events.Add(new FeaturedEvent
                {
                    Title = "TechDays Keynote",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 14, 13, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 14, 14, 30, 0, DateTimeKind.Utc),
                    LocationName = "General Session",
                    IsAllDay = false,
                });

            Events.Add(new FeaturedEvent
                {
                    Title = "Happy Hour",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 14, 22, 30, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                    LocationName = "Expo Hall",
                    IsAllDay = false,
                    Sponsor = sponsorList.FirstOrDefault(x => x.Name == "Microsoft")
                });


            Events.Add (new FeaturedEvent {
                Title = "Darwin Lounge",
                Description = "Stocked full of tech toys, massage chairs, Xamarin engineers, and a few really cool surprises, the Darwin Lounge is your place to hang out and relax between sessions. Back by popular demand, we’ll have several code challenges—Mini-Hacks—for you to complete to earn awesome prizes.",
                StartTime = new DateTime (2017, 10, 14, 12, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime (2017, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                LocationName = "Darwin Lounge",
                IsAllDay = false,
            });

            Events.Add (new FeaturedEvent {
                Title = "TechDays Party",
                Description = $"No lines, just fun! {EventInfo.EventName} is throwing an unforgettable celebration at Xpirit HQ on {EventInfo.StartOfConference}.",
                StartTime = new DateTime (2017, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime (2017, 10, 13, 4, 0, 0, DateTimeKind.Utc),
                LocationName = "Xpirit, Wibautstraat 210, Amsterdam",
                IsAllDay = false,
            });


            Events.Add(new FeaturedEvent
                {
                    Title = "Breakfast",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 12, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 13, 0, 0, DateTimeKind.Utc),
                    LocationName = "Meals",
                    IsAllDay = false,
                });

            Events.Add (new FeaturedEvent {
                Title = "Darwin Lounge",
                Description = "Stocked full of tech toys, massage chairs, Xamarin engineers, and a few really cool surprises, the Darwin Lounge is your place to hang out and relax between sessions. Back by popular demand, we’ll have several code challenges—Mini-Hacks—for you to complete to earn awesome prizes.",
                StartTime = new DateTime (2017, 10, 13, 12, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime (2017, 10, 13, 20, 0, 0, DateTimeKind.Utc),
                LocationName = "Darwin Lounge",
                IsAllDay = false,

            });

            Events.Add(new FeaturedEvent
                {
                    Title = "General Session",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 13, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 14, 30, 0, DateTimeKind.Utc),
                    LocationName ="General Session",
                    IsAllDay = false,
                });


            Events.Add(new FeaturedEvent
                {
                    Title = "Closing Session & Xammy Awards",
                    Description = "",
                    StartTime = new DateTime(2017, 10, 13, 20, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2017, 10, 13, 21, 30, 0, DateTimeKind.Utc),
                    LocationName="General Session",
                    IsAllDay = false,

                });
			return true;
        }

        public override async Task<IEnumerable<FeaturedEvent>> GetItemsAsync(bool forceRefresh = false, Dictionary<string, string> param = null)
        {
			await InitializeStore().ConfigureAwait(false);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject (Events);
            return Events;
        }
    }
}

