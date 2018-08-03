using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NQuotes;
using static forexAI.Logger;

namespace forexAI
{
	public static class App
	{
		// MqlApi object
		public static MqlApi mqlApi = null;
		public static Version version = null;
		public static string currentNetworkId = string.Empty;
		public static Config config = null;

		static App()
		{
			log($"[static Core created mqlApi={mqlApi} fx=1]", "App.full");
			version = Assembly.GetExecutingAssembly().GetName().Version;
			config = new Config();
		}
	}
}