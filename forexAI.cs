using System;
using Color = System.Drawing.Color;
using NQuotes;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;
using FANNCSharp.Double;

namespace forexAI
{
    //+------------------------------------------------------------------+
    //|                                               Moving Average.mq4 |
    //|                      Copyright © 2005, MetaQuotes Software Corp. |
    //|                                       http://www.metaquotes.net/ |
    //+------------------------------------------------------------------+
    public class ForexAI : MqlApi
    {
        private const int MAGICMA = 20050610;

        //private double DecreaseFactor = 3;
        //private double MaximumRisk = 0.02;
        //private int MovingPeriod = 12;
        //private int MovingShift = 6;
        private int previousBars = 0;

        public Random random = new Random();

        public void LoadNetwork(string dirName)
        {
            log($"Loading fann network [{dirName}]");

            NeuralNet ai = new NeuralNet($"d:\\temp\\forexAI\\{dirName}\\FANN.net");

            log($"network: hash={ai.GetHashCode()} inputs={ai.InputCount} outputs={ai.OutputCount} neurons={ai.TotalNeurons}");
            log($"net={dump(ai)}");
        }

        public override int deinit()
        {
            log("Deinitializing ...");
            log("Balance " + AccountBalance());
            log("done");
            return 0;
        }

        public override int init()
        {
            info("Initializing .... ");

            DirectoryInfo d = new DirectoryInfo(@"D:\temp\forexAI");//Assuming Test is your Folder
            DirectoryInfo[] Files = d.GetDirectories("*"); //Getting Text files
            int num = 0;
            foreach (DirectoryInfo file in Files)
            {
                info($"network #{num++} {file.Name}");
                if (random.Next(2) == 1)
                {
                    LoadNetwork(file.Name);
                    break;
                }
            }

            info("done");
            return 0;
        }

        //+------------------------------------------------------------------+
        //| Start function                                                   |
        //+------------------------------------------------------------------+
        public override int start()
        {
            //---- check for history and trading
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