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
    public class LivePrices
    {
        public MqlApi forexAIProgram  = new MqlApi();

        public double[] GetOpen(int numData)
        {
            double[] prices = new double[(forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars)];

            for (int i = 0; i < (forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars); i++)
                prices[i] = forexAIProgram.iOpen(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);
            log($"GetOpen({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }

        public double[] GetClose(int numData)
        {
            double[] prices = new double[(forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars)];

            for (int i = 0; i < (forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars); i++)
                prices[i] = forexAIProgram.iClose(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);
            log($"GetClose({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }

        public double[] GetHigh(int numData)
        {
            double[] prices = new double[(forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars)];

            for (int i = 0; i < (forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars); i++)
                prices[i] = forexAIProgram.iHigh(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);
            log($"GetHigh({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }

        public double[] GetLow(int numData)
        {
            double[] prices = new double[(forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars)];

            for (int i = 0; i < (forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars); i++)
                prices[i] = forexAIProgram.iLow(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);

            log($"GetLow({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }

        public double[] GetVolume(int numData)
        {
            double[] prices = new double[(forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars)];

            for (int i = 0; i < (forexAIProgram.Bars >= numData ? numData : forexAIProgram.Bars); i++)
                prices[i] = forexAIProgram.iVolume(forexAIProgram.Symbol(), MqlApi.PERIOD_M15, i);
            log($"GetVolume({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }
    }
}
