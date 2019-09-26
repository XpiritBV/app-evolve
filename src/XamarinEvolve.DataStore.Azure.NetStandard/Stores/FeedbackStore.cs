using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using XamarinEvolve.DataObjects;

using XamarinEvolve.DataStore.Azure;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.DataStore.Azure
{
    public class FeedbackStore : BaseStore<Feedback>, IFeedbackStore
    {
		public FeedbackStore() : base (new DependencyServiceWrapper())
		{ }

		public FeedbackStore(IDependencyService dependencyService) : base(dependencyService)
		{ }

		public async Task<bool> LeftFeedback(Session session)
        {
			await InitializeStore().ConfigureAwait(false);
            var items = await Table.Where(s => s.SessionId == session.Id).ToListAsync().ConfigureAwait (false);
            return items.Count > 0;
        }

        public Task DropFeedback()
        {
            return Task.FromResult(true);
        }

     

        public override string Identifier => "Feedback";
         
    }
}

