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
		public static MysqlDatabase mysqlDatabase = null;
		public static HashSet<Order> ordersHistory = new HashSet<Order>(100);
		public static HashSet<Order> ordersActive = new HashSet<Order>(10);
		public static DirectoryInfo[] networksDirectories = null;
	}
}
