//......................../´¯)........... 
//.....................,/..../............ 
//..................../..../ ............. 
//............./´¯/' .../´¯/ ¯/\...... 
//........../'/.../... ./... /..././¯\.... 
//........('(....(.... (....(.. /'...).... 
//.........\................. ..\/..../.... 
//..........\......................./´..... 
//............\................ ..(........

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;
using static Newtonsoft.Json.JsonConvert;

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
			catch(IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if(stream != null)
					stream.Close();
			}

			return false;
		}

		public static void Each<T>(IEnumerable<T> items, Action<T> action)
		{
			foreach(var item in items)
				action(item);
		}

		public static void ShowMemoryUsage()
		{
			console($"WorkingSet={(Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0).ToString("0.00")}mb " +
				 $"PrivateMemory={(Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0).ToString("0.00")}mb " +
				 $"Threads={Process.GetCurrentProcess().Threads.Count}");
		}

		public static void DumpToFile(string fileName, object value)
		{
			File.WriteAllText(fileName, SerializeObject(value, Newtonsoft.Json.Formatting.Indented));
		}

		public static string Dump(object obj, int recursion = 1)
		{
			StringBuilder result = new StringBuilder();

			// Protect the method against endless recursion
			if(recursion < 15)
			{
				// Determine object type
				Type t = obj.GetType();

				// Get array with properties for this object
				PropertyInfo[] properties = t.GetProperties();

				foreach(PropertyInfo property in properties)
				{
					try
					{
						// Get the property value
						object value = property.GetValue(obj, null);

						// Create indenting string to put in front of properties of a deeper level
						// We'll need this when we display the property name and value
						string indent = String.Empty;
						string spaces = "|   ";
						string trail = "|...";

						if(recursion > 0)
							indent = new StringBuilder(trail).Insert(0, spaces, recursion - 1).ToString();

						if(value != null)
						{
							// If the value is a string, add quotation marks
							string displayValue = value.ToString();
							if(value is string)
								displayValue = String.Concat('"', displayValue, '"');

							// Add property name and value to return string
							result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, displayValue);

							try
							{
								if(!(value is ICollection))
								{
									// Call var_dump() again to list child properties
									// This throws an exception if the current property value
									// is of an unsupported type (eg. it has not properties)
									result.Append(Dump(value, recursion + 1));
								}
								else
								{
									// 2009-07-29: added support for collections
									// The value is a collection (eg. it's an arraylist or generic list)
									// so loop through its elements and dump their properties
									int elementCount = 0;
									foreach(object element in ((ICollection) value))
									{
										string elementName = String.Format("{0}[{1}]", property.Name, elementCount);
										indent = new StringBuilder(trail).Insert(0, spaces, recursion).ToString();

										// Display the collection element name and type
										result.AppendFormat("{0}{1} = {2}\n", indent, elementName, element.ToString());

										// Display the child properties
										result.Append(Dump(element, recursion + 2));
										elementCount++;
									}

									result.Append(Dump(value, recursion + 1));
								}
							}
							catch { }
						}
						else
						{
							// Add empty (null) property to return string
							result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, "null");
						}
					}
					catch
					{
						// Some properties will throw an exception on property.GetValue()
						// I don't know exactly why this happens, so for now i will ignore them...
					}
				}
			}

			return result.ToString();
		}

		public static void ZeroArray(double[] input)
		{
			for(var i = 0; i < input.Length; i++)
				input[i] = 0.0;
		}

		public static void ZeroArray(int[] input)
		{
			for(var i = 0; i < input.Length; i++)
				input[i] = 0;
		}
	}
}
