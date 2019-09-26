using Xamarin.Forms;

namespace XamarinEvolve.Utils
{
    public class DependencyServiceWrapper : IDependencyService
    {
        public T Get<T>() where T : class
        {
            // The wrapper will simply pass everything through to the real Xamarin.Forms DependencyService class when not unit testing
            return DependencyService.Get<T>();
        }
    }
}