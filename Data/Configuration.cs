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
        public static string rootDirectory = @"d:\temp\forexAI";
        public static string logFileName = $@"{Configuration.rootDirectory}\mt4EA.log";
        public static string settingsPath = $@"{Configuration.rootDirectory}\mt4EA.cfg";
        public static string randomLogFileName = $@"{Configuration.rootDirectory}\seed";
        public static string yrandomLogFileName = $@"{Configuration.rootDirectory}\Yseed";
        public static string mysqlDatabase = "forex";
        public static string mysqlPassword = "secret";
        public static string mysqlServer = "192.168.10.10";
        public static string mysqlUid = "homestead";
        public static string memcachedIP = "192.168.10.10";
        public static double orderLots = 0.01;
        public static double minNegativeSpendProfit = -15.0;
        public static double trailingBorder = 20;
        public static double trailingStop = 24;
        public static bool useMemcached = false;
        public static bool useMysql = false;
        public static bool useAudio = true;
        public static int memcachedPort = 11211;
        public static int magickNumber = 0xC34;
        public static int maxSellBuyOrdersParallel = 0;
       
        // ♫ ♫ ♫ √
        // audio/FX
        public static string audioDirectory = @"d:\Audio_FX";
        public static string newDayWAV = $@"{Configuration.audioDirectory}\17130__noisecollector__ak47-chamber-round.wav";
        public static string profitWAV = $@"{Configuration.audioDirectory}\209578__zott820__cash-register-purchase.wav";
        public static string brokenWAV = $@"{Configuration.audioDirectory}\115276__xdrav__broken-bulb.wav";
        public static string lowBalanceWAV = $@"{Configuration.audioDirectory}\0838.wav";
        public static string goodWorkWAV = $@"{Configuration.audioDirectory}\0816.wav";

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