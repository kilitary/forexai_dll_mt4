﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace forexAI.Tools
{
	public static class Helpers
	{
		public static bool IsFileLocked(string filePath, int secondsToWait = 0)
		{
			bool isLocked = true;
			int i = 0;

			while (isLocked && ((i < secondsToWait) || (secondsToWait == 0)))
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
					i++;

					if (secondsToWait != 0)
						new System.Threading.ManualResetEvent(false).WaitOne(1000);
				}
			}

			return isLocked;
		}
	}
}
