using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.ConsoleColor;
using static Newtonsoft.Json.JsonConvert;

namespace forexAI
{
	public static class Logger
	{
		[DllImport("Kernel32", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
		private static extern int GetCurrentThreadId();

		public static void EraseLogs(params string[] fileNames)
		{
			foreach (var fileName in fileNames)
			{
				console($"Erasing {fileName} ... ");
				if (fileName != null
					&& File.Exists(fileName)
					&& !Helpers.IsFileBusy(fileName))
					File.Delete(fileName);
			}
		}

		public static void ClearLogs()
		{
			var logFiles = new DirectoryInfo(Configuration.rootDirectory).GetFiles("*.log").ToList<FileInfo>();

			logFiles.ForEach(delegate (FileInfo logFile)
			{
				console($"Clearing {logFile.FullName} ...");
				if (!Helpers.IsFileBusy(logFile.FullName))
					File.WriteAllText($@"{logFile.FullName}", "***\r\n");
				else
					console($"file {logFile.FullName} busy!");
			});
		}

		public static void dump(object data, string prefix = "", string fileName = null)
		{
			if (fileName == null)
				fileName = "debug";

			JsonSerializerSettings jsonSettings = new JsonSerializerSettings
			{
				MaxDepth = 5,
				Formatting = Formatting.Indented,
				PreserveReferencesHandling = PreserveReferencesHandling.All
			};

			try
			{
				using (StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true))
				{
					file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " <" +
						Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " +
						((prefix.Length > 0 ? (" " + prefix + ": \r\n") : "") +
						SerializeObject(data, jsonSettings)));
				}
			}
			catch (Exception e)
			{
				console($"dump() exception: {e.Message}");
			}
		}

		public static string console(string lines, ConsoleColor bgcolor = Black, ConsoleColor fgcolor = White)
		{
			Console.BackgroundColor = bgcolor;
			Console.ForegroundColor = fgcolor;
			string logString = DateTime.Now.ToString("HH:mm:ss.fff") + " <" +
				Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines;
			Console.WriteLine(logString);
			Console.ResetColor();
			return logString;
		}


		public static void debug(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "debug";
			try
			{
				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " <" +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines);
				file.Close();
			}
			catch (Exception e)
			{
				console($"exception: {e.Message}");
			}
		}

		public static void error(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "error";
			try
			{
				StackFrame callStack = new StackFrame(1, true);
				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.ffff") + " <" +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + "ERROR: " +
					callStack.GetFileName() + ":" + callStack.GetFileLineNumber() + $" in {callStack.GetMethod().Name}(): " + lines);
				file.Close();
			}
			catch (Exception e)
			{
				console($"exception: {e.Message}");
			}
		}

		public static void info(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "info";
			try
			{
				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " <" +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines);
				file.Close();
			}
			catch (Exception e)
			{
				console($"exception: {e.Message}");
			}
		}

		public static void log(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = Configuration.logFileName;
			try
			{
				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " <" +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines);
				file.Close();
			}
			catch (Exception e)
			{
				console($"exception: {e.Message}");
			}
		}

		public static void consolelog(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = Configuration.logFileName;
			try
			{
				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " <" +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines);
				file.Close();

				console(lines, ConsoleColor.Black, ConsoleColor.Cyan);
			}
			catch (Exception e)
			{
				console($"exception: {e.Message}");
			}
		}

		public static void warning(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "warning";
			try
			{
				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " <" +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + "warning: " + lines);
				file.Close();
			}
			catch (Exception e)
			{
				console($"exception: {e.Message}");
			}
		}

		public static void notice(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "notice";
			try
			{
				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " <" +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + "warning: " + lines);
				file.Close();
			}
			catch (Exception e)
			{
				console($"exception: {e.Message}");
			}
		}
	}
}
