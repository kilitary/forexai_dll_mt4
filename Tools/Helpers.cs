using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;

namespace forexAI
{
	public static class Helpers
	{
		public static bool IsFileBusy(string fileName)
		{
			var targetFileInfo = new FileInfo(fileName);
			FileStream stream = null;

			try
			{
				stream = targetFileInfo.Open(FileMode.Open, FileAccess.Write, FileShare.None);
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			return false;
		}

		public static void ShowMemoryUsage()
		{
			log($"WorkingSet={(Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0).ToString("0.00")}mb " +
				 $"PrivateMemory={(Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0).ToString("0.00")}mb " +
				 $"Threads={Process.GetCurrentProcess().Threads.Count} FileName={Process.GetCurrentProcess().MainModule.ModuleName}");
		}
	}
}
