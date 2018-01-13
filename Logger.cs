using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace forexAI
{
    public static class Logger
    {
        public static void ClearLogs()
        {
            if (File.Exists(Configuration.LogFileName))
                File.Delete(Configuration.LogFileName);
        }

        public static void dump(object data)
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

            //jsonSettings.MaxDepth = 5;
            jsonSettings.Formatting = Formatting.Indented;
            jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;

            try
            {
                debug(JsonConvert.SerializeObject(data, jsonSettings));
            }
            catch (Exception e)
            {
                error($"dump(): {e.Message} [{e.StackTrace}]");
            }
        }

        public static void debug(string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + "debug: " + lines);
            file.Close();
        }

        public static void error(string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + "error: " + lines);
            file.Close();
        }

        public static void info(string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + "info: " + lines);
            file.Close();
        }

        public static void log(string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + lines);
            file.Close();
        }

        public static void warning(string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + "warning: " + lines);
            file.Close();
        }
    }
}
