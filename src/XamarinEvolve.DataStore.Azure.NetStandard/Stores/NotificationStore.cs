using System;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
	public class NotificationStore : BaseStore<Notification>, INotificationStore
	{
		public NotificationStore() : base (new DependencyServiceWrapper())
		{ }

		public NotificationStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

		public async Task<Notification> GetLatestNotification()
		{
			await PullLatestAsync().ConfigureAwait(false);
			var items = await Table.OrderByDescending(n => n.Date).Take(1).ToListAsync().ConfigureAwait(false);
			return items.FirstOrDefault();
		}

		public override async Task<IEnumerable<Notification>> GetItemsAsync(bool forceRefresh = false, Dictionary<string, string> param = null)
		{
			var server = await base.GetItemsAsync(forceRefresh).ConfigureAwait(false);
			if (server.Count() == 0)
			{
				var items = new[]
					{
					new Notification
					{
						Date = Clock.Now.AddDays(-2),
						Text = $"Don't forget to favorite your sessions so you are ready for {EventInfo.EventName}!"
					}
				};
				return items;
			}
			return server.OrderByDescending(s => s.Date);
		}

		public override string Identifier => "Notification";
	}
}

