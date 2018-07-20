﻿using System;
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
		public int ticket;
		public int magickNumber;
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

		public double currentProfit => profit + commission + swap;

		public override string ToString()
		{
			return $"{type} order #{ticket} profit {currentProfit}";
		}

		public int findCounterOrder(List<Order> activeOrders)
		{
			foreach (var order in activeOrders)
			{
				if (order.ticket == ticket || order.counterOrder != null)
					continue;

				if (order.currentProfit < 0.0)
				{
					counterOrder = order;
				}
			}

			if (counterOrder != null)
				return counterOrder.ticket;
			else
				return 0;
		}
	}
}
