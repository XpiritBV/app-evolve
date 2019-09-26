using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace XamarinEvolve.Utils.Helpers
{
	public static class ResourceLoader
	{
		public static string GetEmbeddedResourceString(Assembly assembly, string filename)
		{ 
			var resourceNames = assembly.GetManifestResourceNames();
			var fqFileName = resourceNames.Where(name => name.Contains(filename)).FirstOrDefault();
			if (string.IsNullOrEmpty(fqFileName))
				throw new ArgumentException($"file {filename} not found as embeded resource.");

			Stream stream = assembly.GetManifestResourceStream(fqFileName);
			string text = "";
			using (var reader = new System.IO.StreamReader(stream))
			{
				text = reader.ReadToEnd();
			}
			return text;
		}
	}
}
