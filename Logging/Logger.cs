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
				console($"unlink {fileName} ... ");
				if (fileName != null && File.Exists(fileName) && !Helpers.IsFileBusy(fileName))
					File.Delete(fileName);
			}
		}

		public static void ClearLogs()
		{
			var logFiles = new DirectoryInfo(Configuration.rootDirectory).GetFiles("*.log").ToList<FileInfo>();

			foreach(var logFile in logFiles)
			{
				if (logFile.FullName.Contains("full"))
				{
					console($"skip {logFile.FullName} ...");
					continue;
				}

				if (Helpers.IsFileBusy(logFile.FullName))
					console($"busy {logFile.FullName}");
				else
				{
					console($"truncate {logFile.FullName} ...");
					File.WriteAllText(logFile.FullName, $"***{DateTime.Now.ToString("HH:mm:ss.fff")}***\r\n");
				}

			}
		}

		public static void dump(object data, string prefix = "", string fileName = null)
		{
			string dataValue;
			string sPrefix = prefix.Length > 0 ? " " + prefix + ": \r\n" : "\r\n";

			if (fileName == null)
				fileName = "debug";

			JsonSerializerSettings jsonSettings = new JsonSerializerSettings
			{
				MaxDepth = 5,
				PreserveReferencesHandling = PreserveReferencesHandling.All,
				Formatting = Formatting.Indented
			};

			dataValue = SerializeObject(data, jsonSettings);

			try
			{
				using (StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true))
				{
					file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " +
						Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + " " +
						sPrefix + dataValue);
				}
			}
			catch (Exception e)
			{
				console($"dump() exception: {e.Message}");
			}
		}

		public static void console(string lines, ConsoleColor bgcolor = Black, ConsoleColor fgcolor = White)
		{
			Console.BackgroundColor = bgcolor;
			Console.ForegroundColor = fgcolor;
			string logString = DateTime.Now.ToString("HH:mm:ss.fff") + " " +
				Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + " " + lines;
			Console.WriteLine(logString);
			Console.ResetColor();
		}


		public static void debug(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "debug";
			try
			{
				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + " " + lines);
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
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.ffff") + " " +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + " " + "ERROR: " +
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
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + " " + lines);
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
				if (!File.Exists(Configuration.rootDirectory + "/" + fileName + ".log"))
					File.AppendAllText(Configuration.rootDirectory + "/" + fileName + ".log", $"+++ {fileName} +++\r\n");

				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + " " + lines);
				file.Close();
			}
			catch (Exception e)
			{
				console($"exception: {e.Message}");
			}
		}

		public static void logIf(bool ifCase, string lines, string fileName = null)
		{
			if (ifCase)
				log(lines, fileName);
		}

		public static void consolelog(string lines, string fileName = null, ConsoleColor color = ConsoleColor.Cyan)
		{
			if (fileName == null)
				fileName = Configuration.logFileName;
			try
			{
				StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + " " + lines);
				file.Close();

				console(lines, ConsoleColor.Black, color);
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
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + " " + "warning: " + lines);
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
				file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " +
					Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + " " + "warning: " + lines);
				file.Close();
			}
			catch (Exception e)
			{
				console($"exception: {e.Message}");
			}
		}
	}
}
