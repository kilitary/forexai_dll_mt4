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

namespace forexAI
{
    public class Prices
    {
        public ForexAI forexAIProgram;

        public double[] GetOpen(int numData)
        {
            double[] prices = new double[numData];

            for (int i = 0; i < numData; i++)
                prices[i] = forexAIProgram.iOpen(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);

            dump(prices, "open");
            System.Threading.Thread.Sleep(10000);
            return prices;
        }

        public double[] GetClose(int numData)
        {
            double[] prices = new double[numData];

            for (int i = 0; i < numData; i++)
                prices[i] = forexAIProgram.iClose(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);

            return prices;
        }

        public double[] GetHigh(int numData)
        {
            double[] prices = new double[numData];

            for (int i = 0; i < numData; i++)
                prices[i] = forexAIProgram.iHigh(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);

            return prices;
        }

        public double[] GetLow(int numData)
        {
            double[] prices = new double[numData];

            for (int i = 0; i < numData; i++)
                prices[i] = forexAIProgram.iLow(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);

            return prices;
        }

        public double[] GetVolume(int numData)
        {
            double[] prices = new double[numData];

            for (int i = 0; i < numData; i++)
                prices[i] = forexAIProgram.iVolume(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);

            return prices;
        }
    }
}
