using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Constants;

namespace forexAI
{
	class Order
	{
		public int ticket;
		public OrderType type;
		public DateTime openTime;
		public string symbol;
		public double lots;
		public double profit;
		public double commission;
		public double swap;
		public double stopLoss;
		public double openPrice;
		public string comment;
		public double ageInMinutes;
		public double takeProfit;
		public DateTime expiration;
		public int magickNumber;

		public double currentProfit
		{
			get
			{
				return profit + commission + swap;
			}
		}
	}
}
