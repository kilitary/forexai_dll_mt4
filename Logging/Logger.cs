﻿using System;
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

namespace forexAI
{
    public static class Logger
    {
        [DllImport("Kernel32", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        private static extern int GetCurrentThreadId();

        public static void ClearLogs()
        {
            if (File.Exists(Configuration.LogFileName))
                File.Delete(Configuration.LogFileName);
        }

        public static void dump(object data, string prefix = "", int maxDepth = 255)
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

            jsonSettings.MaxDepth = maxDepth;
            jsonSettings.Formatting = Formatting.Indented;
            jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.All;

            try
            {
                debug((prefix.Length > 0 ? "-- dump of " + prefix + ":\r\n" : "") +
                    JsonConvert.SerializeObject(data, jsonSettings));
            }
            catch (Exception e)
            {
                error($"dump(): {e.Message}");
            }
        }

        public static void debug(string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " <" +
                Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + "debug: " + lines);
            file.Close();
        }

        public static void error(string lines)
        {
            StackFrame callStack = new StackFrame(1, true);
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " <" +
                Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + "error " +
                callStack.GetFileName() + ":" + callStack.GetFileLineNumber() + "# " + lines);
            file.Close();
        }

        public static void info(string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + "<" +
                Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + "info: " + lines);
            file.Close();
        }

        public static void log(string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " <" +
                Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + lines);
            file.Close();
        }

        public static void warning(string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " <" +
                Process.GetCurrentProcess().Id + ":" + GetCurrentThreadId() + "> " + " " + "warning: " + lines);
            file.Close();
        }
    }
}
