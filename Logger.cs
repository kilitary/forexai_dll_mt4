using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
    public static class Logger
    {
        public static void ClearLogs ()
        {
            if(File.Exists(Configuration.LogFileName))
                File.Delete(Configuration.LogFileName);
        }

        public static void debug (string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + "debug: " + lines);
            file.Close();
        }

        public static string dump (object value, string name = "unknown", int depth = 0, bool show = false)
        {
            StringBuilder result = new StringBuilder();

            string spaces = "|   ";
            string indent = new StringBuilder().Insert(0, spaces, depth).ToString();
            string displayValue = string.Empty;

            try
            {
                if(value != null)
                {
                    Type type = value.GetType();

                    displayValue = value.ToString();

                    log($"type={type.ToString()} dv={displayValue}");

                    if(value is bool)
                        displayValue = $"{type.FullName}({(value.Equals(true) ? "True" : "False")})";
                    else if(value is short ||
                        value is ushort ||
                             value is int ||
                        value is uint ||
                             value is long ||
                        value is ulong ||
                             value is float ||
                        value is double ||
                             value is decimal)
                        displayValue = $"{type.FullName}({displayValue})";
                    else if(value is char)
                        displayValue = $"{type.FullName} \"{displayValue}\"";
                    else if(value is char[])
                        displayValue = $"{type.FullName}(#{(value as char[]).Length}) \"{displayValue}\"";
                    else if(value is string)
                        displayValue = $"{type.FullName}(#{(value as string).Length}) \"{displayValue}\"";
                    else if(value is byte || value is sbyte)
                        displayValue = $"{type.FullName} 0x{value:X}";
                    else if(value is byte[] || value is sbyte[])
                        displayValue = $"{type.FullName}(#{(value as byte[]).Length}) 0x{BitConverter.ToString(value as byte[])}";
                    else if(value is ICollection)
                    {
                        var i = 0;
                        var displayValues = string.Empty;

                        var collection = value as ICollection;
                        foreach(object element in collection)
                        {
                            displayValues = string.Concat(displayValues, dump(element, i.ToString(), depth + 1, false));
                            i++;
                        }

                        displayValue = $"{type.FullName}(#{collection.Count}) {{\n{displayValues}{indent}}}";
                    }
                    else
                    {
                        var displayValues = string.Empty;

                        PropertyInfo[] properties = type.GetProperties();
                        foreach(PropertyInfo property in properties)
                            displayValues = string.Concat(displayValues,
                                dump(property.GetValue(value, null), property.Name, depth + 1, false));

                        displayValue = $"{type.Name}(#{properties.Length}) {{\n{displayValues}{indent}}}\n";
                    }
                }
                else
                    displayValue = "null";

                if(name != string.Empty)
                    result.Append(indent + ' ' + name + " => " + displayValue + "\n");
                else
                    result.Append(indent + displayValue + "\n");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result.ToString();
        }

        public static void error (string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + "error: " + lines);
            file.Close();
        }

        public static void info (string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + "info: " + lines);
            file.Close();
        }

        public static void log (string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + lines);
            file.Close();
        }

        public static void warning (string lines)
        {
            StreamWriter file = new StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " " + "warning: " + lines);
            file.Close();
        }
    }
}
