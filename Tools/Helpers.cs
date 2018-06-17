using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;

namespace forexAI.Tools
{
	public static class Helpers
	{
		public static bool IsFileLocked(string filePath, int secondsToWait = 0)
		{
			bool isLocked = true;
			int iterator = 0;

			while (isLocked && ((iterator < secondsToWait) || (secondsToWait == 0)))
			{
				try
				{
					using (File.Open(filePath, FileMode.Open)) { }
					return false;
				}
				catch (IOException e)
				{
					var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
					isLocked = errorCode == 32 || errorCode == 33;
					iterator++;

					if (secondsToWait != 0)
						new System.Threading.ManualResetEvent(false).WaitOne(1000);
				}
			}

			return isLocked;
		}

		public static void ShowMemoryUsage()
		{
			log($"WorkingSet={(Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0).ToString("0.00")}mb " +
				 $"PrivateMemory={(Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0).ToString("0.00")}mb " +
				 $"Threads={Process.GetCurrentProcess().Threads.Count} FileName={Process.GetCurrentProcess().MainModule.ModuleName}");
		}
	}
}
