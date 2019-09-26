using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;
using XamarinEvolve.Utils.Helpers;

namespace XamarinEvolve.Clients.Portable.Auth
{
	public class XamarinSSOClient : ISSOClient
	{
		const string XamarinSSOApiKey = "0c833t3w37jq58dj249dt675a465k6b0rz090zl3jpoa9jw8vz7y6awpj5ox0qmb";

		readonly HttpClient _client;
		readonly IStoreManager _storeManager;

        public XamarinSSOClient() : this (new DependencyServiceWrapper())
        {
            
        }

		public XamarinSSOClient(IDependencyService locator)
		{
            _storeManager = locator.Get<IStoreManager>();

			_client = HttpClientFactory.CreateClient(locator);
			_client.BaseAddress = new Uri("https://auth.xamarin.com/api/v1/");
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{XamarinSSOApiKey}:"));
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
		}

		async Task<string> PostForm(string endpoint, IDictionary<string, string> keyValues)
		{
			var response = await _client.PostAsync(endpoint, new FormUrlEncodedContent(keyValues));
			return await response.Content.ReadAsStringAsync();
		}

		public async Task<AccountResponse> CreateToken(string email, string password)
		{
			var json = await PostForm("auth", new Dictionary<string, string>
				{
					{"email", email},
					{"password", password},
				});
			return JsonConvert.DeserializeObject<AccountResponse>(json);
		}

		#region ISSOClient implementation

		public Task<AccountResponse> LoginAsync(string username, string password) =>
			CreateToken(username, password);

		public Task<AccountResponse> LoginAnonymouslyAsync(string impersonateUserId)
		{
//#if ENABLE_TEST_CLOUD
			return _storeManager.LoginAnonymouslyAsync(impersonateUserId);
//#else
			throw new InvalidOperationException("Xamarin SSO Client does not support anonymous login. Set FeatureFlags.LoginEnabled to false");
//#endif
		}

		public Task LogoutAsync()
		{
			return Task.FromResult(0);
		}


		#endregion
	}
}
