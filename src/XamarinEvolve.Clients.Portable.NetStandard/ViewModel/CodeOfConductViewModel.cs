using System.Reflection;
using XamarinEvolve.Utils;
using XamarinEvolve.Utils.Helpers;

namespace XamarinEvolve.Clients.Portable
{
	public class CodeOfConductViewModel : ViewModelBase
	{
		public static string CodeOfConductContent = ResourceLoader.GetEmbeddedResourceString(Assembly.Load(new AssemblyName("XamarinEvolve.Clients.Portable.NetStandard")), "codeofconduct.txt");

		public string CodeOfConduct => CodeOfConductContent;

		public string PageTitle => AboutThisApp.CodeOfConductPageTitle;
	}
}

