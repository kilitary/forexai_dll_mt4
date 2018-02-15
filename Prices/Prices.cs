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
using forexAI.Api;
namespace forexAI
{
    public class LivePrices
    {
        public Mql pMql  = new Mql();

        public double[] GetOpen(int numData)
        {
            double[] prices = new double[(pMql.Bars >= numData ? numData : pMql.Bars)];

            for (int i = 0; i < (pMql.Bars >= numData ? numData : pMql.Bars); i++)
                prices[i] = pMql.iOpen(pMql.Symbol(), MqlApi.PERIOD_M15, i);
            log($"GetOpen({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }

        public double[] GetClose(int numData)
        {
            double[] prices = new double[(pMql.Bars >= numData ? numData : pMql.Bars)];

            for (int i = 0; i < (pMql.Bars >= numData ? numData : pMql.Bars); i++)
                prices[i] = pMql.iClose(pMql.Symbol(), MqlApi.PERIOD_M15, i);
            log($"GetClose({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }

        public double[] GetHigh(int numData)
        {
            double[] prices = new double[(pMql.Bars >= numData ? numData : pMql.Bars)];

            for (int i = 0; i < (pMql.Bars >= numData ? numData : pMql.Bars); i++)
                prices[i] = pMql.iHigh(pMql.Symbol(), MqlApi.PERIOD_M15, i);
            log($"GetHigh({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }

        public double[] GetLow(int numData)
        {
            double[] prices = new double[(pMql.Bars >= numData ? numData : pMql.Bars)];

            for (int i = 0; i < (pMql.Bars >= numData ? numData : pMql.Bars); i++)
                prices[i] = pMql.iLow(pMql.Symbol(), MqlApi.PERIOD_M15, i);

            log($"GetLow({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }

        public double[] GetVolume(int numData)
        {
            double[] prices = new double[(pMql.Bars >= numData ? numData : pMql.Bars)];

            for (int i = 0; i < (pMql.Bars >= numData ? numData : pMql.Bars); i++)
                prices[i] = pMql.iVolume(pMql.Symbol(), MqlApi.PERIOD_M15, i);
            log($"GetVolume({numData}) = {JsonConvert.SerializeObject(prices)}");
            return prices;
        }
    }
}
