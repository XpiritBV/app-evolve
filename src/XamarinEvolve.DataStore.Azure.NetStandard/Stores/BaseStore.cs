using System;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.DataObjects;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices;
using System.Diagnostics;
using Plugin.Connectivity;
using XamarinEvolve.Utils;
using System.Linq;

namespace XamarinEvolve.DataStore.Azure
{
    public class BaseStore<T> : IBaseStore<T> where T : class, IBaseDataObject, new()
    {
		Stopwatch _timer = new Stopwatch();

		[Conditional("DEBUG")]
		protected void StartTimer()
		{
			_timer.Reset();
			_timer.Start();
		}

		[Conditional("DEBUG")]
		protected void DumpTiming(string message)
		{
			_timer.Stop();
			Debug.WriteLine($"{message} - {_timer.ElapsedMilliseconds}ms");
			_timer.Restart();
		}

		IStoreManager storeManager;
        protected IDependencyService Locator { get; }

        public BaseStore() : this(new DependencyServiceWrapper())
        {
            
        }

        public BaseStore(IDependencyService locator)
        {
            Locator = locator;
        }

        public virtual string Identifier => "Items";

        IMobileServiceSyncTable<T> table;
        protected IMobileServiceSyncTable<T> Table
        {
            get { return table ?? (table = StoreManager.MobileService.GetSyncTable<T>()); }
          
        }

        public void DropTable()
        {
            table = null;
        }

        #region IBaseStore implementation

        public async Task<bool> InitializeStore()
        {
			bool result;
			if (storeManager == null)
            {
                storeManager = Locator.Get<IStoreManager>();
            }

			if (!storeManager.IsInitialized)
			{
				result = await storeManager.InitializeAsync().ConfigureAwait(false);
				return result;
			}
			else
			{
				return true;
			}
        }

        public virtual async Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false, Dictionary<string, string> param = null)
        {
            await InitializeStore().ConfigureAwait (false);
			if (forceRefresh)
			{
				await SyncAsync().ConfigureAwait(false);
				await PullLatestAsync().ConfigureAwait(false);
			}
			return await Table.ToEnumerableAsync().ConfigureAwait(false);
        }

        public virtual async Task<T> GetItemAsync(string id)
        {
            await InitializeStore().ConfigureAwait(false);
            var items = await Table.Where(s => s.Id == id).ToListAsync().ConfigureAwait(false);
            return items?.FirstOrDefault();
        }

        public virtual async Task<bool> InsertAsync(T item)
        {
            await InitializeStore().ConfigureAwait(false);
            await Table.InsertAsync(item).ConfigureAwait(false);
            await SyncAsync().ConfigureAwait (false);
            return true;
        }

        public virtual async Task<bool> UpdateAsync(T item)
        {
            await InitializeStore().ConfigureAwait(false);
            await Table.UpdateAsync(item).ConfigureAwait(false);
            await SyncAsync().ConfigureAwait (false);
            return true;
        }

        public virtual async Task<bool> RemoveAsync(T item)
        {
            await InitializeStore().ConfigureAwait(false);
            await Table.DeleteAsync(item).ConfigureAwait(false);
            await SyncAsync().ConfigureAwait (false);
            return true;
        }

        public async Task<bool> PullLatestAsync(Dictionary<string,string> param=null)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                Debug.WriteLine("Unable to pull items, we are offline");
                return false;
            }
            try
            {
				await InitializeStore().ConfigureAwait(false);
				if (param!= null)
				{
					await Table.PullAsync($"all{Identifier}", Table.CreateQuery().WithParameters(param)).ConfigureAwait(false);
				}
				else
				{
					await Table.PullAsync($"all{Identifier}", Table.CreateQuery()).ConfigureAwait(false);
				}
			}
			catch (MobileServicePushFailedException pex)
			{
				Debug.WriteLine($"Unable to pull items for {Identifier}, that is alright as we have offline capabilities: {pex}");
				Debug.WriteLine($"Push status: {pex.PushResult.Status}");
				foreach (var error in pex.PushResult.Errors)
				{
					Debug.WriteLine($"--{error.TableName} : {error.RawResult}");
				}
				return false;
			}
			catch (Exception ex)
            {
				Debug.WriteLine($"Unable to pull items for {Identifier}, that is alright as we have offline capabilities: {ex}");
                return false;
            }
            return true;
        }

        public async Task<bool> SyncAsync()
        {
            Debug.WriteLine($"Syncing {this.Identifier}...");
            if (!CrossConnectivity.Current.IsConnected)
            {
                Debug.WriteLine("Unable to sync items, we are offline");
                return false;
            }
			try
			{
				await InitializeStore().ConfigureAwait(false);
				Debug.WriteLine($"PushAsync {this.Identifier}...");

				await StoreManager.MobileService.SyncContext.PushAsync().ConfigureAwait(false);
				
                Debug.WriteLine($"PullLatestAsync {this.Identifier}...");
				var ok = await PullLatestAsync().ConfigureAwait(false);
			}
			catch (MobileServicePushFailedException pex)
			{
				Debug.WriteLine($"Unable to sync items for {Identifier}, that is alright as we have offline capabilities: {pex}");
				Debug.WriteLine($"Push status: {pex.PushResult.Status}");
				foreach (var error in pex.PushResult.Errors)
				{
					Debug.WriteLine($"--{error.TableName} : {error.RawResult}");
				}
				return false;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to sync items for {Identifier}, that is alright as we have offline capabilities: {ex}");
                return false;
            }
            finally
            {
				Debug.WriteLine($"Done {this.Identifier}");
			}
			return true;
        }

        #endregion
    }
}

