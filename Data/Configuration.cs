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
    public static class Configuration
    {
        // common configuration
        public static string dataDirectory = @"D:\temp\forexAI";
        public static string logFileName = @"d:\temp\forexAI\mt4.log";
        public static string settingsPath = @"d:\temp\forexAI\mt4forexai.cfg";
        public static string mysql_database = "forex";
        public static string mysql_password = "secret";
        public static string mysql_server = "192.168.10.10";
        public static string mysql_uid = "homestead";
        public static string MemcachedIP = "192.168.10.10";
        public static bool useMemcached = false;
        public static bool useMysql = false;
        public static int MemcachedPort = 11211;
        public static int MagickNumber = 0xCB;

        // audio/fx
        public static string newDayWAV = @"d:\Audio_FX\17130__noisecollector__ak47-chamber-round.wav";
        public static string profitWAV = @"d:\Audio_FX\209578__zott820__cash-register-purchase.wav";
        public static string brokenWAV = @"d:\Audio_FX\115276__xdrav__broken-bulb.wav";
        public static string lowBalanceWAV = @"d:\Audio_FX\0838.wav";
        public static string goodWorkWAV = @"d:\Audio_FX\0816.wav";

        //░░ ♡ ▄▀▀▀▄░░░ 
        //▄███▀░◐░░░▌░░░░░░░ 
        //░░░░▌░░░░░▐░░░░░░░ 
        //░░░░▐░░░░░▐░░░░░░░ 
        //░░░░▌░░░░░▐▄▄░░░░░ 
        //░░░░▌░░░░▄▀▒▒▀▀▀▀▄ 
        //░░░▐░░░░▐▒▒▒▒▒▒▒▒▀▀▄ 
        //░░░▐░░░░▐▄▒▒▒▒▒▒▒▒▒▒▀▄ 
        //░░░░▀▄░░░░▀▄▒▒▒▒▒▒▒▒▒▒▀▄ 
        //░░░░░░▀▄▄▄▄▄█▄▄▄▄▄▄▄▄▄▄▄▀▄ 
        // Experimental features
        public static int ExperimentalAlliedRandomLimit = 4499;
    }
}

//                                                   |    |    |

//-------^-----------------^^-------------^------- ѼΞΞΞΞΞΞΞD -----------------------^---------