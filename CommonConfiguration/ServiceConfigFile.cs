using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonConfiguration
{
	public static class ServiceConfigFile
	{
		public static IEnumerable<string> ReadFile(string name)
		{
			using (Stream resourceStream =
				typeof(ServiceConfigFile).Assembly.GetManifestResourceStream("CommonConfiguration." + name + ".txt"))
			{
				StreamReader reader = new StreamReader(resourceStream);
				string currentLine;
				while ((currentLine = reader.ReadLine()) != null)
				{
					currentLine = currentLine.Trim();
					if (currentLine.StartsWith("#")) // Comment
					{
						continue;
					}
					if (currentLine == "")
					{
						continue;
					}
					yield return currentLine;
				}
				reader.Close();
			}
		}
	}
}
