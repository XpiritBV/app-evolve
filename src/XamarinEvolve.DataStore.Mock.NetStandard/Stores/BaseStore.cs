using System;
using XamarinEvolve.DataStore.Abstractions;
using System.Threading.Tasks;
using XamarinEvolve.Utils;
using System.Collections.Generic;

namespace XamarinEvolve.DataStore.Mock
{
    public class BaseStore<T> : IBaseStore<T>
    {
        protected IDependencyService Locator { get; }

        public BaseStore() : this(new DependencyServiceWrapper())
        {
            
        }

        public BaseStore(IDependencyService locator)
        {
            Locator = locator;
        }

        #region IBaseStore implementation

        public void DropTable()
        {
            
        }
        public virtual System.Threading.Tasks.Task<bool> InitializeStore()
        {
            throw new NotImplementedException();
        }
        public virtual System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<T>> GetItemsAsync(bool forceRefresh = false, Dictionary<string, string> param = null)
        {
            throw new NotImplementedException();
        }
        public virtual System.Threading.Tasks.Task<T> GetItemAsync(string id)
        {
            throw new NotImplementedException();
        }
        public virtual System.Threading.Tasks.Task<bool> InsertAsync(T item)
        {
            throw new NotImplementedException();
        }
        public virtual System.Threading.Tasks.Task<bool> UpdateAsync(T item)
        {
            throw new NotImplementedException();
        }
        public virtual System.Threading.Tasks.Task<bool> RemoveAsync(T item)
        {
            throw new NotImplementedException();
        }
        public virtual System.Threading.Tasks.Task<bool> SyncAsync()
        {
            return Task.FromResult(true);
        }

        public string Identifier => "store";
        #endregion
    }
}

