using System.Net.Http;

namespace XamarinEvolve.Utils.Helpers
{
	public static class HttpClientFactory
	{
		public static HttpClient CreateClient(IDependencyService locator)
		{
			HttpClient client;
			var messageHandlerProvider = locator.Get<IMessageHandlerProvider>();
			if (messageHandlerProvider != null)
			{
				client = new HttpClient(messageHandlerProvider?.GetHandler());
			}
			else
			{
				client = new HttpClient();
			}

			return client;
		}
	}
}
