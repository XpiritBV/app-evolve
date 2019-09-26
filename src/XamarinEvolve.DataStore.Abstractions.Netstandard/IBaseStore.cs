using System;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace XamarinEvolve.DataStore.Abstractions
{
    public interface IBaseStore<T>
    {
        Task<bool> InitializeStore();
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false, Dictionary<string, string> param = null);
        Task<T> GetItemAsync(string id);
        Task<bool> InsertAsync(T item);
        Task<bool> UpdateAsync(T item);
        Task<bool> RemoveAsync(T item);
        Task<bool> SyncAsync();

        void DropTable();

        string Identifier { get; }
    }
}

