using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using FANNCSharp.Double;
using Newtonsoft.Json;
using NQuotes;
using static forexAI.Logger;
using static Newtonsoft.Json.JsonConvert;

namespace forexAI
{
	public class ChartPrices
	{
		public double[] GetOpen(int numData, int Bars)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = App.mqlApi.Open[i];

			return prices;
		}

		public double[] GetClose(int numData, int Bars)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = App.mqlApi.Close[i];

			return prices;
		}

		public double[] GetHigh(int numData, int Bars)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = App.mqlApi.High[i];

			return prices;
		}

		public double[] GetLow(int numData, int Bars)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = App.mqlApi.Low[i];

			return prices;
		}

		public double[] GetVolume(int numData, int Bars)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = App.mqlApi.Volume[i];

			return prices;
		}
	}
}
