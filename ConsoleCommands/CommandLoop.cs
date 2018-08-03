using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;

namespace forexAI
{
	public static class ConsoleCommandReceiver
	{
		public static void CommandLoop()
		{
			string typing = String.Empty;
			string[] commandParts = null;
			var resultString = string.Empty;
			int nBytes = 0;

			while (true)
			{
				try
				{
					typing = Console.ReadLine().Trim();
					commandParts = typing.Split(' ');
					if (commandParts.Count() <= 0)
						continue;

					log($"command: {typing}");
					consolelog($"> {typing} ({commandParts.Count()})", "dev", ConsoleColor.DarkGreen);

					switch (commandParts[0])
					{
						case "version":
							resultString = $"version: {App.version.ToString()}";
							break;
						case "config":
							if (commandParts.Count() >= 1)
							{
								if (commandParts.Count() == 1)
									resultString = $"config: {App.config.DumpString()}";
								else
								{
									switch (commandParts[1])
									{
										case "save":
											nBytes = App.config.Save();
											resultString = $"config saved ({nBytes} bytes)";
											break;
										case "clear":
											App.config.Clear();
											App.config.Save();
											resultString = $"config clear";
											break;
										default:
											if (commandParts.Count() > 2)
											{
												log($"setting {commandParts[1]} to {commandParts[2]}", "dev");
												App.config[commandParts[1]] = commandParts[2];
												resultString = $"set {commandParts[1]} = {commandParts[2]}";
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
					consolelog($"< {resultString}", "dev", ConsoleColor.White);
				}
				catch (Exception e)
				{
					log($"EXCEPTION COMMAND '{Newtonsoft.Json.JsonConvert.SerializeObject(commandParts)}: '{e.Message}: {e.StackTrace}", "error");
				}
			}
		}
	}
}
