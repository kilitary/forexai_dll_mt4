using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
    internal static class Logger
    {
        public static void log(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.ff") + " " + lines);
            file.Close();
        }

        public static void info(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.ff") + " " + "info: " + lines);
            file.Close();
        }

        public static void warning(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.ff") + " " + "warning: " + lines);
            file.Close();
        }

        public static void error(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.ff") + " " + "error: " + lines);
            file.Close();
        }

        public static void debug(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(Configuration.LogFileName, true);
            file.WriteLine(DateTime.Now.ToString("h:mm:ss.ff") + " " + "debug: " + lines);
            file.Close();
        }

        public static string dump(object value, string name = "unknown", int depth = 0, bool show = false)
        {
            StringBuilder result = new StringBuilder();

            string spaces = "|   ";
            string indent = new StringBuilder().Insert(0, spaces, depth).ToString();
            string displayValue = String.Empty;

            try
            {
                if (value != null)
                {
                    Type type = value.GetType();

                    displayValue = value.ToString();

                    log($"type={type.ToString()} dv={displayValue}");

                    if (value is Boolean)
                    {
                        displayValue = String.Format("{0}({1})", type.FullName, value.Equals(true) ? "True" : "False");
                    }
                    else if (value is Int16 || value is UInt16 ||
                             value is Int32 || value is UInt32 ||
                             value is Int64 || value is UInt64 ||
                             value is Single || value is Double ||
                             value is Decimal)
                    {
                        displayValue = String.Format("{0}({1})", type.FullName, displayValue);
                    }
                    else if (value is Char)
                    {
                        displayValue = String.Format("{0} \"{1}\"", type.FullName, displayValue);
                    }
                    else if (value is Char[])
                    {
                        displayValue = String.Format("{0}(#{1}) \"{2}\"", type.FullName, (value as Char[]).Length,
                            displayValue);
                    }
                    else if (value is String)
                    {
                        displayValue = String.Format("{0}(#{1}) \"{2}\"", type.FullName, (value as String).Length,
                            displayValue);
                    }
                    else if (value is Byte || value is SByte)
                    {
                        displayValue = String.Format("{0} 0x{1:X}", type.FullName, value);
                    }
                    else if (value is Byte[] || value is SByte[])
                    {
                        displayValue = String.Format("{0}(#{1}) 0x{2}", type.FullName, (value as Byte[]).Length, BitConverter.ToString(value as Byte[]));
                    }
                    else if (value is ICollection)
                    {
                        var i = 0;
                        var displayValues = String.Empty;

                        var collection = value as ICollection;
                        foreach (object element in collection)
                        {
                            displayValues = String.Concat(displayValues, dump(element, i.ToString(), depth + 1, false));
                            i++;
                        }

                        displayValue = String.Format("{0}(#{1}) {{\n{2}{3}}}", type.FullName, collection.Count, displayValues, indent);
                    }
                    else
                    {
                        var displayValues = String.Empty;

                        PropertyInfo[] properties = type.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            displayValues = String.Concat(displayValues,
                                dump(property.GetValue(value, null), property.Name, depth + 1, false));
                        }

                        displayValue = String.Format("{0}(#{1}) {{\n{2}{3}}}\n", type.Name, properties.Length, displayValues, indent);
                    }
                }
                else
                {
                    displayValue = "null";
                }

                if (name != String.Empty)
                {
                    result.Append(indent + ' ' + name + " => " + displayValue + "\n");
                }
                else
                {
                    result.Append(indent + displayValue + "\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result.ToString();
        }
    }
}