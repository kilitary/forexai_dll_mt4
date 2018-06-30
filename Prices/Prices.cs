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
	public class LivePrices
	{
		public double[] GetOpen(int numData, int Bars, IMqlArray<double> Open)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = Open[i];

			return prices;
		}

		public double[] GetClose(int numData, int Bars, IMqlArray<double> Close)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = Close[i];

			return prices;
		}

		public double[] GetHigh(int numData, int Bars, IMqlArray<double> High)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = High[i];

			return prices;
		}

		public double[] GetLow(int numData, int Bars, IMqlArray<double> Low)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = Low[i];

			return prices;
		}

		public double[] GetVolume(int numData, int Bars, IMqlArray<double> Volume)
		{
			double[] prices = new double[(Bars >= numData ? numData : Bars)];

			for (int i = 0; i < (Bars >= numData ? numData : Bars); i++)
				prices[i] = Volume[i];

			return prices;
		}
	}
}
