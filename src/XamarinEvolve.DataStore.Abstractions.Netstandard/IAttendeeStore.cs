using System;
using System.Threading.Tasks;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.DataStore.Abstractions
{
	public interface IAttendeeStore : IBaseStore<Attendee>
	{
		Task<bool> SubmitRegistration(Attendee data);
		Task<bool> IsRegistered();
	}
}
