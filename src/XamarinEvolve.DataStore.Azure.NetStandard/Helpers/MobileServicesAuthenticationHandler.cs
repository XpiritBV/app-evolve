using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static XamarinEvolve.DataStore.Azure.StoreManager;

namespace XamarinEvolve.Clients.Portable.NetStandard.Helpers
{
	class MobileServicesAuthenticationHandler : DelegatingHandler
	{
		public IMobileServiceClient Client { get; set; }

	
		public MobileServicesAuthenticationHandler()
		{

		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (this.Client == null)
			{
				throw new InvalidOperationException("Make sure to set the 'Client' property in this handler before using it.");
			}

			// Cloning the request, in case we need to send it again
			var clonedRequest = await CloneRequest(request);
			var response = await base.SendAsync(clonedRequest, cancellationToken);

			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				// Oh noes, user is not logged in - we got a 401
				// Log them in, this time hardcoded with Facebook but you would
				// trigger the login presentation in your application
				try
				{

					var settings = await ReadSettingsAsync().ConfigureAwait(false);
					var impersonateUserId = settings?.UserId; // see if we have a saved user id from a previous token
		
					var credentials = new JObject();
					if (!string.IsNullOrEmpty(impersonateUserId))
					{
						credentials["anonymousUserId"] = impersonateUserId;
					}
					
					var user = await this.Client.LoginAsync("AnonymousUser", credentials).ConfigureAwait(false);
					// we're now logged in again.

					// Clone the request
					clonedRequest = await CloneRequest(request);

					clonedRequest.Headers.Remove("X-ZUMO-AUTH");
					// Set the authentication header
					clonedRequest.Headers.Add("X-ZUMO-AUTH", user.MobileServiceAuthenticationToken);

					// Resend the request
					response = await base.SendAsync(clonedRequest, cancellationToken);
				}
				catch (InvalidOperationException)
				{
					// user cancelled auth, so lets return the original response
					return response;
				}
			}

			return response;
		}

		async Task<StoreSettings> ReadSettingsAsync()
		{
			return (await MobileService.SyncContext.Store.LookupAsync(nameof(StoreSettings), StoreSettings.StoreSettingsId).ConfigureAwait(false))?.ToObject<StoreSettings>();
		}

		private async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
		{
			var result = new HttpRequestMessage(request.Method, request.RequestUri);
			foreach (var header in request.Headers)
			{
				result.Headers.Add(header.Key, header.Value);
			}

			if (request.Content != null && request.Content.Headers.ContentType != null)
			{
				var requestBody = await request.Content.ReadAsStringAsync();
				var mediaType = request.Content.Headers.ContentType.MediaType;
				result.Content = new StringContent(requestBody, Encoding.UTF8, mediaType);
				foreach (var header in request.Content.Headers)
				{
					if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
					{
						result.Content.Headers.Add(header.Key, header.Value);
					}
				}
			}

			return result;
		}
	}
}
