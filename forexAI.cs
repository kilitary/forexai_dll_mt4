﻿using System;
using Color = System.Drawing.Color;
using NQuotes;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;
using FANNCSharp.Double;
using System.Text.RegularExpressions;

namespace forexAI
{
    public class ForexAI : MqlApi
    {
        public Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds());
        private int inputDimension = 0;
        private string inputLayerActivationFunction, middleLayerActivationFunction;
        private int previousBars = 0;
        public override int deinit()
        {
            log("Deinitializing ...");
            log("Balance " + AccountBalance());
            log("done");
            return 0;
        }

        public override int init()
        {
            log("Initializing .... ");

            LoadNetworks();

            log("done");
            return 0;
        }

        public void LoadNetwork(string dirName)
        {
            log($"Loading fann network [{dirName}]");

            NeuralNet ai = new NeuralNet($"d:\\temp\\forexAI\\{dirName}\\FANN.net");

            info($"network: hash={ai.GetHashCode()} inputs={ai.InputCount} outputs={ai.OutputCount} neurons={ai.TotalNeurons}");

            string fileTextData = File.ReadAllText($"d:\\temp\\forexAI\\{dirName}\\configuration.txt");
            Regex regex = new Regex(@"\[([^ \r\n\[\]]{1,10}?)\s+?", RegexOptions.Multiline | RegexOptions.Singleline);
            foreach (Match match in regex.Matches(fileTextData))
            {
                if (match.Groups[0].Value.Length <= 0)
                    continue;

                string funcName = match.Groups[0].Value.Trim('[', ' ');
                info($" function [{funcName}]");

                Dictionary<string, string> data = new Dictionary<string, string>();

                data["name"] = funcName;

                if (Data.nnFunctions.ContainsKey(funcName))
                    continue;

                Data.nnFunctions.Add(funcName, data);
            }
            Match match2 = Regex.Match(fileTextData, "InputDimension:\\s+(\\d+)?");
            int.TryParse(match2.Groups[1].Value, out inputDimension);

            info($"inputDimension = {inputDimension}");

            Match match3 = Regex.Match(fileTextData, "InputActFunc:\\s+([^ ]{1,40}?)\\s+LayerActFunc:\\s+([^ \r\n]{1,40})",
                 RegexOptions.Singleline);
            info($"activation functions: input [{match3.Groups[1].Value}] layer [{match3.Groups[2].Value}]");

            inputLayerActivationFunction = match3.Groups[1].Value;
            middleLayerActivationFunction = match3.Groups[2].Value;
        }

        public void LoadNetworks()
        {
            DirectoryInfo d = new DirectoryInfo(@"D:\temp\forexAI");//Assuming Test is your Folder
            DirectoryInfo[] Files = d.GetDirectories("*"); //Getting Text files
            int num = 0;
            foreach (DirectoryInfo file in Files)
            {
                info($" > network #{num++} {file.Name}");
                if (random.Next(2) == 1)
                {
                    LoadNetwork(file.Name);
                    break;
                }
            }
        }
        //+------------------------------------------------------------------+
        //| Start function                                                   |
        //+------------------------------------------------------------------+
        public override int start()
        {
            if (Bars == previousBars)
                return 0;

            previousBars = Bars;
            ////---- calculate open orders by current symbol
            //string symbol = Symbol();

            //if (CalculateCurrentOrders() == 0)
            //    CheckForOpen(symbol);
            //else
            //    CheckForClose(symbol);

            return 0;
        }

        //+------------------------------------------------------------------+
        //| Calculate open positions                                         |
        //+------------------------------------------------------------------+
        //private int CalculateCurrentOrders()
        //{
        //    int buys = 0, sells = 0;

        //    for (int i = 0; i < OrdersTotal(); i++)
        //    {
        //        if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
        //            break;
        //        if (OrderSymbol() == Symbol() && OrderMagicNumber() == MAGICMA)
        //        {
        //            if (OrderType() == OP_BUY)
        //                buys++;
        //            if (OrderType() == OP_SELL)
        //                sells++;
        //        }
        //    }

        //    //---- return orders volume
        //    if (buys > 0)
        //        return (buys);
        //    return (-sells);
        //}

        ////+------------------------------------------------------------------+
        ////| Check for close order conditions                                 |
        ////+------------------------------------------------------------------+
        //private void CheckForClose(string symbol)
        //{
        //    //---- go trading only for first tiks of new bar
        //    if (Volume[0] > 1)
        //        return;
        //    //---- get Moving Average
        //    double ma = iMA(symbol, 0, MovingPeriod, MovingShift, MODE_SMA, PRICE_CLOSE, 0);

        //    Print("orders " + OrdersTotal());

        //    for (int i = 0; i < OrdersTotal(); i++)
        //    {
        //        if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
        //            break;
        //        if (OrderMagicNumber() != MAGICMA || OrderSymbol() != Symbol())
        //            continue;
        //        //---- check order type
        //        if (OrderType() == OP_BUY)
        //        {
        //            log("test  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
        //            if (Open[1] > ma && Close[1] < ma)
        //                OrderClose(OrderTicket(), OrderLots(), Bid, 3, Color.White);

        //            break;
        //        }
        //        if (OrderType() == OP_SELL)
        //        {
        //            log("test  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
        //            if (Open[1] < ma && Close[1] > ma)
        //                OrderClose(OrderTicket(), OrderLots(), Ask, 3, Color.White);

        //            break;
        //        }
        //    }
        //}

        ////+------------------------------------------------------------------+
        ////| Check for open order conditions                                  |
        ////+------------------------------------------------------------------+
        //private void CheckForOpen(string symbol)
        //{
        //    //---- go trading only for first tiks of new bar
        //    if (Volume[0] > 1)
        //        return;

        //    //---- get Moving Average
        //    double ma = iMA(symbol, 0, MovingPeriod, MovingShift, MODE_SMA, PRICE_CLOSE, 0);

        //    //---- sell conditions
        //    if (Open[1] > ma && Close[1] < ma)
        //    {
        //        OrderSend(Symbol(), OP_SELL, LotsOptimized(), Bid, 3, 0, 0, "", MAGICMA, DateTime.MinValue, Color.Red);
        //        return;
        //    }
        //    //---- buy conditions
        //    if (Open[1] < ma && Close[1] > ma)
        //    {
        //        OrderSend(Symbol(), OP_BUY, LotsOptimized(), Ask, 3, 0, 0, "", MAGICMA, DateTime.MinValue, Color.Blue);
        //    }
        //}

        ////+------------------------------------------------------------------+
        ////| Calculate optimal lot size                                       |
        ////+------------------------------------------------------------------+
        //private double LotsOptimized()
        //{
        //    // history orders total
        //    int orders = OrdersHistoryTotal();
        //    // number of losses orders without a break
        //    int losses = 0;
        //    //---- select lot size
        //    double lot = NormalizeDouble(AccountFreeMargin() * MaximumRisk / 1000.0, 1);
        //    //---- calcuulate number of losses orders without a break
        //    if (DecreaseFactor > 0)
        //    {
        //        for (int i = orders - 1; i >= 0; i--)
        //        {
        //            if (!OrderSelect(i, SELECT_BY_POS, MODE_HISTORY))
        //            {
        //                Print("Error in history!");
        //                break;
        //            }
        //            if (OrderSymbol() != Symbol() || OrderType() > OP_SELL)
        //                continue;

        //            if (OrderProfit() > 0)
        //                break;
        //            if (OrderProfit() < 0)
        //                losses++;
        //        }
        //        if (losses > 1)
        //            lot = NormalizeDouble(lot - lot * losses / DecreaseFactor, 1);
        //    }
        //    //---- return lot size
        //    if (lot < 0.1)
        //        lot = 0.1;
        //    return (lot);
        //}
    }
}