﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Constants;

namespace forexAI
{
	class Order
	{
		public OrderType type;
		public DateTime openTime;
		public DateTime expiration;
		public int ticket;
		public int magickNumber;
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

		public double currentProfit => profit + commission + swap;
	}
}
