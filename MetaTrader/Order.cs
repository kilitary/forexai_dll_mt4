using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Constants;

namespace forexAI
{
	public class Order
	{
		public OrderType type;
		public DateTime openTime;
		public DateTime expiration;
		public Order counterOrder = null;
		public string symbol;
		public string comment;
		public double lots;
		public double profit;
		public double commission;
		public double swap;
		public double stopLoss;
		public double openPrice;
		public double ageInMinutes;
		public double takeProfit;
		public int ticket;
		public int magickNumber;
		public double calculatedProfit => profit + commission + swap;

		public override string ToString()
		{
			return $"{type} order #{ticket} profit {calculatedProfit}";
		}

		public int FindSpendCounterOrder()
		{
			foreach (var order in Repository.ordersActive)
			{
				if (order.ticket == ticket || order.counterOrder != null)
					continue;

				if (order.calculatedProfit < 0.0)
				{
					counterOrder = order;
					break;
				}
			}

			if (counterOrder != null)
				return counterOrder.ticket;
			else
				return 0;
		}
	}
}
