using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
	public static class Web
	{
		public static string DownloadString(string url)
		{
			WebClient client = new WebClient();
			string reply = client.DownloadString(url);
			return reply;
		}
	}
}
