using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;
using System.Runtime.InteropServices;
using static System.Console;
using System.IO;

namespace forexAI
{
	public static class ConsoleCommandReceiver
	{
		[DllImport("kernel32.dll")]
		private static extern bool FreeConsole();

		static string[] commandLineParts = null;
		static string typing = String.Empty;
		static string resultString = string.Empty;
		static string command = string.Empty;
		static bool runCommandLineParser = true;

		public static void CommandsReadingLoop()
		{
			Console.CursorLeft = 0;
			Console.CursorTop = 0;
			Console.Beep(1650, 33);

			while(runCommandLineParser)
			{
				try
				{
					typing = ReadLine().Trim();
					commandLineParts = typing.Split(' ');
					resultString = string.Empty;

					if(commandLineParts.Count() <= 0 || commandLineParts[0].Length == 0)
						continue;

					log($"command: {typing}", "dev");
					consolelog($"=> {typing}", "dev", ConsoleColor.Gray);

					command = commandLineParts[0].ToLower();

					switch(command)
					{
						case "exit":
							consolelog($"exit called");
							Environment.Exit(0);
							break;

						case "break":
							consolelog($"next optimization step called");
							App.mqlApi.ExpertRemove();
							break;

						case "rotate":
							if(App.currentNetworkId.Length <= 0)
								resultString = $"empty current network dir";
							else if(Directory.Exists(Configuration.rootDirectory + $"\\{App.currentNetworkId}"))
								Directory.Delete(Configuration.rootDirectory + "\\" + App.currentNetworkId, true);

							var dirs = new DirectoryInfo(Configuration.rootDirectory + "\\NEW").GetDirectories("NET_*");
							var rnd = YRandom.between(0, dirs.Length - 1);

                     if(dirs.Length > 0)
                     {
                        resultString = $"next dir chosen (from {dirs.Length} dirs): {dirs[rnd]}";

                        try
                        {
                           Directory.Move(Configuration.rootDirectory + $"\\NEW\\{dirs[rnd]}",
                              Configuration.rootDirectory + $"\\{dirs[rnd]}");
                        }
                        catch(Exception e)
                        {
                           consolelog($"exception: {e.Message}");
                        }
                     }
                     else
                     {
                        resultString = "no new networks";
                     }

							Beep(1000, 20);
							break;

						case "memstats":
							Helpers.ShowMemoryUsage();
							break;

						case "testerstats":
							for(var i = 0; i < 100; i++)
								resultString += $"{i,-4}: {App.mqlApi.TesterStatistics(i)}\r\n";
							break;

						case "clear":
							Clear();
							break;

						case "enable":
							App.config.Enable(commandLineParts[1]);
							resultString = $"enabled {commandLineParts[1]}";
							break;

						case "disable":
							App.config.Disable(commandLineParts[1]);
							resultString = $"disabled {commandLineParts[1]}";
							break;

						case "toggle":
							bool enabled = App.config.Toggle(commandLineParts[1]);
							resultString = $"toggle {commandLineParts[1]}: {enabled}";
							break;

						case "version":
							resultString = $"version: {App.version.ToString()}";
							break;

						case "set":
						case "config":
							if(commandLineParts.Count() >= 1)
							{
								if(commandLineParts.Count() == 1)
									resultString = $"config: {App.config.DumpString()}";
								else
								{
									switch(commandLineParts[1].ToLower())
									{
										case "del":
										case "remove":
										case "unset":
										case "-":
											App.config.Remove(commandLineParts[2].Trim());
											resultString = $"removed {commandLineParts[2].Trim()}";
											break;

										case "save":
											var nBytes = App.config.Save();
											resultString = $"config saved ({nBytes} bytes)";
											break;

										case "reset":
										case "clear":
											App.config.Clear();
											resultString = $"config cleared";
											break;

										default:
											if(commandLineParts.Count() > 2)
											{
												log($"setting {commandLineParts[1].Trim()} to {commandLineParts[2].Trim()}", "dev");
												App.config[commandLineParts[1].Trim()] = commandLineParts[2].Trim();
												resultString = $"set {commandLineParts[1].Trim()} = {commandLineParts[2].Trim()}";
												App.config.Save();
											}
											else
											{
												log($"app.config[{commandLineParts[1].Trim()}", "dev");
												resultString = $"{commandLineParts[1].Trim()} = {App.config[commandLineParts[1].Trim()]}";
											}
											break;
									}
								}
							}
							else
								resultString = $"no config cmd";
							break;

						default:
							resultString = $"unknown command '{command}'";
							break;

					}

					if(resultString.Trim().Contains("unknown command"))
						Beep(190, 15);
					else
						Beep(1850, 55);

					if(resultString.Trim().Length > 0)
						consolelog($"<= {resultString.Trim()}", "dev", ConsoleColor.White);
				}
				catch(Exception e)
				{
					log($"EXCEPTION COMMAND '{Newtonsoft.Json.JsonConvert.SerializeObject(commandLineParts)}: '{e.Message}: {e.StackTrace}", "error");
				}
				finally
				{
					//FreeConsole();
					//log($"console freed", "dev");
				}
			}
		}
	}
}
