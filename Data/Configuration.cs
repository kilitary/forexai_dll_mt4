﻿//╮╰╮╮▕╲╰╮╭╯╱▏╭╭╭╭ 
//╰╰╮╰╭╱▔▔▔▔╲╮╯╭╯ 
//┏━┓┏┫╭▅╲╱▅╮┣┓╭║║║ 
//╰┳╯╰┫┗━╭╮━┛┣╯╯╚╬╝ 
//╭┻╮╱╰╮╰━━╯╭╯╲┊ ║ 
//╰┳┫▔╲╰┳━━┳╯╱▔┊ ║ 
//┈┃╰━━╲▕╲╱▏╱━━━┬╨╮ 
//┈╰━━╮┊▕╱╲▏┊╭━━┴╥╯
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NQuotes;

namespace forexAI
{
	// ♫ ♫ ♫ √
	public static class Configuration
	{
		// common configuration   √  
		public static string rootDirectory = @"c:\forexAI";
		public static string configFilePath = $@"{rootDirectory}\mt4EA.cfg";
		public static string XRandomLogFileName = $@"{rootDirectory}\seed";
		public static string YRandomLogFileName = $@"{rootDirectory}\Yseed";
		public static string logFileName = "mt4EA";
		public static bool unlinkBadNetworksEnabled = false;
		public static int magickNumber = 0xc34;

		// mysql √
		public static bool mysqlEnabled = false;
		public static string mysqlDatabase = "forex";
		public static string mysqlPassword = "secret";
		public static string mysqlServer = "192.168.10.10";
		public static string mysqlUid = "homestead";

		// memcached √
		public static bool memcahedEnabled = false;
		public static string memcachedIP = "192.168.0.100";
		public static int memcachedPort = 11211;

		// audio/FX  ♫ ♫ ♫ √
		public static bool audioEnabled = true;
		public static string audioDirectory = $@"{rootDirectory}\AudioFX";
		public static string newDayWAV = $@"{audioDirectory}\Speech Sleep.wav";
		public static string profitWAV = $@"{audioDirectory}\209578__zott820__cash-register-purchase.wav";
		public static string brokenWAV = $@"{audioDirectory}\115276__xdrav__broken-bulb.wav";
		public static string lowBalanceWAV = $@"{audioDirectory}\0838.wav";
		public static string goodWorkWAV = $@"{audioDirectory}\0816.wav";
		public static string failWav = $@"{audioDirectory}\fail.wav";
		public static string wipeWav = $@"{audioDirectory}\AchievementUnlocked.wav";

		// ♥ ♥ ♥ ♥ √
		//[! ♡ ▄▀▀▀▄░░░ !] 
		//▄███▀░◐░░░▌░░░░[!] 
		//░░░░▌░░░░░▐░░░░░[!]
		//░░░░▐░░░░░▐░░░░░░░ 
		//░░░░▌░░░░░▐▄▄░░░░░ 
		//░░░░▌░░░░▄▀▒▒▀▀▀▀▄ 
		//░░░▐░░░░▐▒▒▒▒▒▒▒▒▀▀▄ 
		//░░░▐░░░░▐▄▒▒▒▒▒▒▒▒▒▒▀▄ 
		//░░░░▀▄░░░░▀▄▒▒▒▒▒▒▒▒▒▒▀▄ 
		//[!]░░░▀▄▄▄▄▄█▄▄▄▄▄▄▄▄▄▄▄▀▄ 
		// Experimental features [!]
		public static bool tryExperimentalFeatures = false;
		public static int experimentalAlliedRandomUpperBound = 4498;
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░  
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		//░[!]░░░░░░░▌▌░▌▌░░░░░ 
		//░░░░░░░░░▄▄▌▌▄▌▌░░[!]  
	}
}

//                                                   |    |    |

//-------^-----------------^^-------------^------- ѼΞΞΞΞΞΞΞD -----------------------^---------