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

		public static void CommandReadingLoop()
		{
			var typing = String.Empty;
			string[] commandParts = null;
			var resultString = string.Empty;
			int nBytes = 0;

			Console.WindowWidth = 180;
			Console.WindowHeight = 30;
			Console.CursorLeft = 0;
			Console.CursorTop = 0;
			Console.Beep(1650, 33);

			while(true)
			{
				try
				{
					typing = ReadLine().Trim();
					commandParts = typing.Split(' ');
					resultString = string.Empty;

					if(commandParts.Count() <= 0 || commandParts[0].Length == 0)
						continue;

					log($"command: {typing}", "dev");
					consolelog($"=> {typing}", "dev", ConsoleColor.Gray);

					switch(commandParts[0])
					{
						case "next":
							if(App.currentNetworkId.Length <= 0)
							{
								resultString = $"empty current network dir";
							}
							else
							{
								if(Directory.Exists(Configuration.rootDirectory + $"\\{App.currentNetworkId}"))
									Directory.Delete(Configuration.rootDirectory + "\\" + App.currentNetworkId, true);
							}

							var dirs = new DirectoryInfo(Configuration.rootDirectory + "\\NEW").GetDirectories("NET_*");
							var rnd = forexAI.YRandom.between(0, dirs.Length - 1);

							resultString = $"next dir chosen (from {dirs.Length} dirs): {dirs[rnd]}";

							try
							{
								Directory.Move(Configuration.rootDirectory + $"\\NEW\\{dirs[rnd]}", Configuration.rootDirectory + $"\\{dirs[rnd]}");
							}
							catch(Exception e)
							{
								consolelog($"exception: {e.Message}");
							}

							Beep(1000, 20);
							break;

						case "memstats":
							forexAI.Helpers.ShowMemoryUsage();
							break;

						case "testerstats":
							for(var i = 0; i < 100; i++)
							{
								resultString += $"{i,-4}: {App.mqlApi.TesterStatistics(i)}\r\n";
							}
							break;

						case "clear":
							Console.Clear();
							break;

						case "enable":
							App.config.Enable(commandParts[1]);
							resultString = $"enabled {commandParts[1]}";
							break;

						case "disable":
							App.config.Disable(commandParts[1]);
							resultString = $"disabled {commandParts[1]}";
							break;

						case "toggle":
							bool enabled = App.config.Toggle(commandParts[1]);
							resultString = $"toggle {commandParts[1]}: {enabled}";
							break;

						case "version":
							resultString = $"version: {App.version.ToString()}";
							break;

						case "config":
							if(commandParts.Count() >= 1)
							{
								if(commandParts.Count() == 1)
									resultString = $"config: {App.config.DumpString()}";
								else
								{
									switch(commandParts[1])
									{
										case "-":
											App.config.Remove(commandParts[2]);
											resultString = $"removed {commandParts[2]}";
											break;

										case "save":
											nBytes = App.config.Save();
											resultString = $"config saved ({nBytes} bytes)";
											break;

										case "clear":
											App.config.Clear();
											resultString = $"config clear";
											break;

										default:
											if(commandParts.Count() > 2)
											{
												log($"setting {commandParts[1]} to {commandParts[2]}", "dev");
												App.config[commandParts[1]] = commandParts[2];
												resultString = $"set {commandParts[1]} = {commandParts[2]}";
												App.config.Save();
											}
											else
											{
												log($"app.config[{commandParts[1]}", "dev");
												resultString = $"{commandParts[1]} = {App.config[commandParts[1]]}";
											}
											break;
									}
								}
							}
							else
								resultString = $"unknown config cmd";
							break;

						default:
							resultString = $"unknown command '{commandParts[0]}'";
							break;

					}

					if(resultString.Contains("unknown command"))
						Beep(190, 15);
					else
						Beep(1850, 55);

					consolelog($"<= {resultString}", "dev", ConsoleColor.White);
				}
				catch(Exception e)
				{
					log($"EXCEPTION COMMAND '{Newtonsoft.Json.JsonConvert.SerializeObject(commandParts)}: '{e.Message}: {e.StackTrace}", "error");
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
