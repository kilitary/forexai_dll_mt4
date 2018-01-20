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
        public static string DataDirectory = @"D:\temp\forexAI";
        public static string LogFileName = @"d:\temp\forexAI\mt4.log";
        public static string mysql_database = "forex";
        public static string mysql_password = "secret";
        public static string mysql_server = "192.168.10.10";
        public static string mysql_uid = "homestead";
        public static bool useMemcached = false;
        public static bool useMysql = true;
        public static string MemcachedIP = "192.168.10.10";
        public static int MemcachedPort = 11211;

        // Experimental features
        public static int ExRandomDimension = 5000;
    }
}
