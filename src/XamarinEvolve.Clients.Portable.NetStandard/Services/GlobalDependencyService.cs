using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
    public class GlobalDependencyService
    {
		public static IDependencyService Locator { get; private set; } = new DependencyServiceWrapper();

		public static void OverrideLocator(IDependencyService locator)
		{
			Locator = locator;
		}
    }
}
