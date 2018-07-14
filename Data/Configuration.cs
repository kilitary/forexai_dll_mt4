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
using NQuotes;

namespace forexAI
{
	// ♫ ♫ ♫ √
	public static class Configuration
	{
		// common configuration    
		public static string rootDirectory = @"d:\forexAI";
		public static string logFileName = "mt4EA";
		public static string configFilePath = $@"{rootDirectory}\mt4EA.cfg";
		public static string XRandomLogFileName = $@"{rootDirectory}\seed";
		public static string YRandomLogFileName = $@"{rootDirectory}\Yseed";
		public static string mysqlDatabase = "forex";
		public static string mysqlPassword = "secret";
		public static string mysqlServer = "192.168.10.10";
		public static string mysqlUid = "homestead";
		public static string memcachedIP = "192.168.0.100";
		public static bool useMemcached = false;
		public static bool useMysql = false;
		public static bool useAudio = true;
		public static int memcachedPort = 11211;
		public static int magickNumber = 0xc34;

		// ♫ ♫ ♫ √
		// audio/FX
		public static string audioDirectory = $@"{rootDirectory}\AudioFX";
		public static string newDayWAV = $@"{audioDirectory}\Speech Sleep.wav";
		public static string profitWAV = $@"{audioDirectory}\209578__zott820__cash-register-purchase.wav";
		public static string brokenWAV = $@"{audioDirectory}\115276__xdrav__broken-bulb.wav";
		public static string lowBalanceWAV = $@"{audioDirectory}\0838.wav";
		public static string goodWorkWAV = $@"{audioDirectory}\0816.wav";
		public static string failWav = $@"{audioDirectory}\fail.wav";
		public static string wipe = $@"{audioDirectory}\0784.wav";

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