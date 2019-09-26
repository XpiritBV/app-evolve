using Plugin.Connectivity;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Utils;
using XamarinEvolve.Utils.Helpers;

[assembly: Dependency(typeof(ImageChecker))]

namespace XamarinEvolve.Clients.Portable
{
    public class ImageChecker : IImageChecker
    {
        private readonly IDependencyService _locator;
        private readonly HttpClient _client;

        public ImageChecker() : this(new DependencyServiceWrapper())
		{
		}

        public ImageChecker(IDependencyService dependencyService)
        {
            _locator = dependencyService;
			_client = HttpClientFactory.CreateClient(dependencyService);
		}

        public async Task<bool> IsImageUsableForAppLink(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
				//prevent freeze of UI, when ofline, when ofline we can't check, so return false
				if (CrossConnectivity.Current.IsConnected)
				{
					var request = new HttpRequestMessage(HttpMethod.Head, url.Replace("http://", "https://"));
					var response = await _client.SendAsync(request).ConfigureAwait(false);
					if (response.IsSuccessStatusCode)
					{
						return response.Content.Headers.ContentLength < (60 * 1024); // Max 60 KB
					}
				}
				else
				{
					return false;
				}
            }
            catch (Exception ex)
            {
                _locator.Get<ILogger>()?.Report(ex);
            }
            return false;
        }
    }
}
