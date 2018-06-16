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

		public static void TruncateLog(string fileName = null)
		{
			if (fileName != null && File.Exists(fileName))
				File.Delete(fileName);
		}

		public static void ClearLogs()
		{
			DirectoryInfo directory = new DirectoryInfo(Configuration.rootDirectory);
			var logs = directory.GetFiles("*.log");

			foreach (var log in logs)
				File.WriteAllText($@"{log.FullName}", "***\r\n");
		}

		public static void dump(object data, string prefix = "", int maxDepth = 3)
		{
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings
			{
				MaxDepth = maxDepth,
				Formatting = Formatting.Indented,
				PreserveReferencesHandling = PreserveReferencesHandling.All
			};

			try
			{
				using (StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/debug.log", true))
				{
					file.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + " <" +
						Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " +
						((prefix.Length > 0 ? "[" + prefix + "] \r\n" : "") +
						SerializeObject(data, jsonSettings)));
				}
			}
			catch (Exception e)
			{
				error($"dump(): {e.Message}");
			}
		}

		public static string console(string lines, ConsoleColor bgcolor = Black, ConsoleColor fgcolor = White)
		{
			Console.BackgroundColor = bgcolor;
			Console.ForegroundColor = fgcolor;
			string logString = DateTime.Now.ToString("hh:mm:ss.fff") + " <" +
				Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines;
			Console.WriteLine(logString);
			Console.ResetColor();
			return logString;
		}


		public static void debug(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "debug";

			StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
			file.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + " <" +
				Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines);
			file.Close();
		}

		public static void error(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "error";

			StackFrame callStack = new StackFrame(1, true);
			StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
			file.WriteLine(DateTime.Now.ToString("h:mm:ss.ffff") + " <" +
				Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + "ERROR: " +
				callStack.GetFileName() + ":" + callStack.GetFileLineNumber() + $" in {callStack.GetMethod().Name}(): " + lines);
			file.Close();
		}

		public static void info(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "info";

			StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
			file.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + " <" +
				Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines);
			file.Close();
		}

		public static void log(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = Configuration.logFileName;

			StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
			file.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + " <" +
				Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines);
			file.Close();
		}

		public static void warning(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "warning";

			StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
			file.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + " <" +
				Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + "warning: " + lines);
			file.Close();
		}

		public static void notice(string lines, string fileName = null)
		{
			if (fileName == null)
				fileName = "notice";

			StreamWriter file = new StreamWriter(Configuration.rootDirectory + "/" + fileName + ".log", true);
			file.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + " <" +
				Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + "warning: " + lines);
			file.Close();
		}
	}
}
