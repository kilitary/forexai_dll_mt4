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
		public static string DownloadUrl(string url)
		{
			return new WebClient().DownloadString(url);
		}
	}
}
