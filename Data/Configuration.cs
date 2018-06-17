//╮╰╮╮▕╲╰╮╭╯╱▏╭╭╭╭ 
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

namespace forexAI
{
	// ♫ ♫ ♫ √
	public static class Configuration
	{
		// common configuration    
		public static string rootDirectory = @"d:\forexAI";
		public static string logFileName = $@"mt4EA";
		public static string settingsPath = $@"{rootDirectory}\mt4EA.cfg";
		public static string randomLogFileName = $@"{rootDirectory}\seed";
		public static string yrandomLogFileName = $@"{rootDirectory}\Yseed";
		public static string mysqlDatabase = "forex";
		public static string mysqlPassword = "secret";
		public static string mysqlServer = "192.168.10.10";
		public static string mysqlUid = "homestead";
		public static string memcachedIP = "192.168.0.100";
		public static double orderLots = 0.01;
		public static double minNegativeSpendProfit = -8.0;
		public static double trailingBorder = 30;
		public static double trailingStop = 20;
		public static bool useMemcached = false;
		public static bool useMysql = false;
		public static bool useAudio = true;
		public static bool useDynamicOptimizedLots = false;
		public static int memcachedPort = 11211;
		public static int magickNumber = 0xC34;
		public static int maxOrdersInParallel = 6;
		public static int minStableTrendBarForEnter = 4;
		public static int maxStableTrendBarForEnter = 10;
		public static int resetStableTrendBarAtBar = 110;
		public static int minTradePeriodBars = 4;

		// ♫ ♫ ♫ √
		// audio/FX
		public static string audioDirectory = $@"{rootDirectory}\Audio_FX";
		public static string newDayWAV = $@"{audioDirectory}\17130__noisecollector__ak47-chamber-round.wav";
		public static string profitWAV = $@"{audioDirectory}\209578__zott820__cash-register-purchase.wav";
		public static string brokenWAV = $@"{audioDirectory}\115276__xdrav__broken-bulb.wav";
		public static string lowBalanceWAV = $@"{audioDirectory}\0838.wav";
		public static string goodJobWAV = $@"{audioDirectory}\0816.wav";

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
		public static bool tryExperimentalFeatures = false; // √
		//░░░░░░░░░░░▌▌░▌▌░░░░░  
		public static int experimentalAlliedRandomUpperBound = 4498; // √
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