using System.Linq;
using System.Threading.Tasks;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
	public class AttendeeStore : BaseStore<Attendee>, IAttendeeStore
	{
		public override string Identifier => "Attendee";

		public async Task<bool> SubmitRegistration(Attendee data)
		{
			var result = await InitializeStore().ConfigureAwait(false);

			var items = await Table.Where(a => a.UserId == Settings.Current.UserIdentifier).ToListAsync();
			if (items.Any())
			{
				var item = items.First();
				item.Email = data.Email;
				item.Name = data.Name;
				result = await UpdateAsync(item).ConfigureAwait(false);
			}
			else
			{
				data.UserId = Settings.Current.UserIdentifier;
				result = await InsertAsync(data).ConfigureAwait(false);
			}

			result = await SyncAsync().ConfigureAwait(false);
			return result;
		}

		public async Task<bool> IsRegistered()
		{
			await InitializeStore().ConfigureAwait(false);
			var items = await Table.ReadAsync().ConfigureAwait(false);
			return items.Any();
		}

	}
}
