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

		static App()
		{
			log($"[static Core created mqlApi={mqlApi}]", "App.full");
			version = Assembly.GetExecutingAssembly().GetName().Version;
		}
	}
}