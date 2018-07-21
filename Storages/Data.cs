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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
	public static class Data
	{
		public static Dictionary<string, Dictionary<string, string>> nnFunctions = new Dictionary<string, Dictionary<string, string>>();
		public static MysqlDatabase mysqlDatabase = null;
		public static List<Order> historyOrders = new List<Order>();
		public static List<Order> activeOrders = new List<Order>();
		public static DirectoryInfo[] networkDirs = null;
	}
}
