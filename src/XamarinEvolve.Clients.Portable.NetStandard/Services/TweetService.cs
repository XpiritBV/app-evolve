using Plugin.Share;
using Plugin.Share.Abstractions;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable.Services;
using XamarinEvolve.Utils;

[assembly: Dependency(typeof(TweetService))]

namespace XamarinEvolve.Clients.Portable.Services
{
    public class TweetService : ITweetService
    {
        public async Task InitiateConferenceTweet()
        {
            var shareMessage = new ShareMessage
            {
                Text = EventInfo.HashTag + " "
            };

            ShareUIActivityType[] excludedShareUITypes = 
            {
                ShareUIActivityType.AddToReadingList,
                ShareUIActivityType.AssignToContact,
                ShareUIActivityType.OpenInIBooks,
                ShareUIActivityType.PostToFlickr,
                ShareUIActivityType.PostToVimeo,
                ShareUIActivityType.Print,
                ShareUIActivityType.SaveToCameraRoll
            };

            await CrossShare.Current.Share(shareMessage, new ShareOptions { ChooserTitle = $"Share {EventInfo.EventName}", ExcludedUIActivityTypes = excludedShareUITypes });
        }
    }
}
