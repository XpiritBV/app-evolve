using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.DataStore.Azure;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable.Auth.Azure
{
	public sealed class XamarinSSOClient : ISSOClient
	{
		private readonly IStoreManager _storeManager;
		private readonly ILogger _logger;
        private readonly IDependencyService _locator;

        public XamarinSSOClient (IDependencyService locator)
        {
            _locator = locator;
        }

        public XamarinSSOClient() : this (new DependencyServiceWrapper())
		{
			_storeManager = _locator.Get<IStoreManager>();
			_logger = _locator.Get<ILogger>();

			if (_storeManager == null)
			{
				throw new InvalidOperationException($"The {typeof(XamarinSSOClient).FullName} requires a {typeof(StoreManager).FullName}.");
			}
		}

		public async Task<AccountResponse> LoginAsync(string username, string password)
		{
			return await _storeManager.LoginAsync(username, password);
		}

		public async Task<AccountResponse> LoginAnonymouslyAsync(string impersonateUserId)
		{
			try
			{
				return await _storeManager.LoginAnonymouslyAsync(impersonateUserId);
			}
			catch (Exception e)
			{
				_logger.Report(e, "Method", "LoginAnonymouslyAsync", Severity.Error);
				return null;
			}
		}

		public async Task LogoutAsync()
		{
			await _storeManager.LogoutAsync();
		}
	}
}
