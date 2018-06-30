//......................../´¯)........... 
//.....................,/..../............ 
//..................../..../ ............. 
//............./´¯/' .../´¯/ ¯/\...... 
//........../'/.../... ./... /..././¯\.... 
//........('(....(.... (....(.. /'...).... 
//.........\................. ..\/..../.... 
//..........\......................./´..... 
//............\................ ..(........

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
	public static class Constants
	{
		public enum TrendDirection
		{
			Up,
			Down
		}

		public enum OrderType
		{
			Sell,
			SellLimit,
			SellStop,
			Buy,
			BuyLimit,
			BuyStop

		}
	}
}
