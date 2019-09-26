using System;
using System.Threading.Tasks;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;

namespace XamarinEvolve.DataStore.Mock
{
	public class AttendeeStore : BaseStore<Attendee>, IAttendeeStore
	{
		public async Task<bool> IsRegistered()
		{
			var item = await GetItemAsync(XamarinEvolve.Utils.Settings.Current.UserIdentifier).ConfigureAwait(false);
			return item != null;
		}

		public async Task<bool> SubmitRegistration(Attendee data)
		{
			var item = await GetItemAsync(XamarinEvolve.Utils.Settings.Current.UserIdentifier).ConfigureAwait(false);

			if (item != null)
			{
				item.Name = data.Name;
				item.Email = data.Email;
				await UpdateAsync(item).ConfigureAwait(false);
			}
			else
			{
				data.UserId = Utils.Settings.Current.UserIdentifier;
				await InsertAsync(data);
			}
			return true;
		}
	}
}
